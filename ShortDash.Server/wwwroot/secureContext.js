var secureContext = (function () {
    // Internal functions
    var _ = {
        getClientPublicKey: function () {
            return localStorage.getItem("SecureContextClientPublicKey");
        },

        getClientPrivateKey: function () {
            return localStorage.getItem("SecureContextClientPrivateKey");
        },

        getServerPublicKey: function () {
            return localStorage.getItem("SecureContextServerPublicKey");
        },

        getSessionkey: function () {
            var key = sessionStorage.getItem("SecureContextSessionKey");
            return forge.util.decode64(key);
        },

        rsaDecrypt: function (value) {
            var privateKeyPem = this.getClientPrivateKey();
            var privateKey = forge.pki.privateKeyFromPem(privateKeyPem);
            var decodedBytes = forge.util.decode64(value);
            return privateKey.decrypt(decodedBytes);
        },

        rsaEncrypt: function (value) {
            var publicKeyPem = this.getServerPublicKey();
            var publicKey = forge.pki.publicKeyFromPem(publicKeyPem);
            var encryptedBytes = publicKey.encrypt(value);
            return forge.util.encode64(encryptedBytes);
        },

        setSessionKey: function (encryptedKey) {
            var key = this.rsaDecrypt(encryptedKey);
            sessionStorage.setItem("SecureContextSessionKey", key);
        },

        setServerPublicKey: function (publicKey) {
            localStorage.setItem("SecureContextServerPublicKey", publicKey);
        },
    };

    // Initialize context
    if (_.getClientPublicKey() === null) {
        var keypair = forge.pki.rsa.generateKeyPair({ bits: 2048, e: 0x10001 });
        var privateKey = forge.pki.privateKeyToPem(keypair.privateKey);
        var publicKey = forge.pki.publicKeyToPem(keypair.publicKey);
        localStorage.setItem("SecureContextClientPrivateKey", privateKey);
        localStorage.setItem("SecureContextClientPublicKey", publicKey);
    }

    // Public functions
    return {
        initChannel: function (serverPublicKey, base64data) {
            _.setServerPublicKey(serverPublicKey);
            var challengeIndex = base64data.indexOf(":");
            var challengeType = "N/A";
            if (challengeIndex >= -1) challengeType = base64data.substring(0, challengeIndex);
            if (challengeType !== "RSA") throw "Unsupported challenge type: " + challengeType;
            base64data = base64data.substring(challengeIndex + 1);
            var decryptedChallenge = _.rsaDecrypt(base64data);
            var encryptedChallenge = _.rsaEncrypt(decryptedChallenge);
            return encryptedChallenge;
        },

        decrypt: function (base64data) {
            var data = forge.util.decode64(base64data);
            var dataHex = forge.util.bytesToHex(data);
            var iv = forge.util.hexToBytes(dataHex.slice(0, 32));
            var encryptedBytes = forge.util.hexToBytes(dataHex.slice(32));
            var buffer = forge.util.createBuffer(encryptedBytes, "raw");
            var decipher = forge.cipher.createDecipher("AES-CBC", _.getSessionkey());
            decipher.start({ iv: iv });
            decipher.update(buffer);
            decipher.finish();
            return forge.util.decodeUtf8(decipher.output);
        },

        encrypt: function (data) {
            var buffer = forge.util.createBuffer(data, "utf8");
            var iv = forge.random.getBytesSync(16);
            var cipher = forge.cipher.createCipher("AES-CBC", _.getSessionkey());
            cipher.start({ iv: iv });
            cipher.update(buffer);
            cipher.finish();
            var encrypted = cipher.output;
            var encryptedHex = forge.util.bytesToHex(iv) + encrypted.toHex();
            var encryptedBytes = forge.util.hexToBytes(encryptedHex);
            return forge.util.encode64(encryptedBytes);
        },

        exportPublicKey: function () {
            return _.getClientPublicKey();
        },

        initSecureInputText: function (id, dotnetHelper, initialValue) {
            var inputText = document.getElementById(id);
            var secureContext = this;
            inputText.value = this.decrypt(initialValue);
            var updateEvent = function () {
                dotnetHelper.invokeMethodAsync("OnClientChanged", secureContext.encrypt(inputText.value));
            }
            inputText.onchange = updateEvent;
            inputText.oninput = updateEvent;
        },

        openChannel: function (encryptedKey) {
            _.setSessionKey(encryptedKey);
        },

        setSecureInputTextValue: function (id, encryptedValue) {
            var inputText = document.getElementById(id);
            inputText.value = this.decrypt(encryptedValue);
        },

        setSecureControlText: function (id, encryptedValue) {
            var control = document.getElementById(id);
            control.innerText = this.decrypt(encryptedValue);
        },

        setSecureQRCode: function (id, encryptedValue, width, height) {
            new QRCode(document.getElementById(id), {
                text: this.decrypt(encryptedValue),
                width: width,
                height: height
            });
        },
    };
})();
window.secureContext = secureContext;