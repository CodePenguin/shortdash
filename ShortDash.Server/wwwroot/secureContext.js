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
            return CryptoJS.enc.Base64.parse(key);
        },

        rsaDecrypt: function (value) {
            var privateKey = this.getClientPrivateKey();
            var crypto = new JSEncrypt();
            crypto.setPrivateKey(privateKey);
            var encrypted = crypto.decrypt(value);
            return encrypted;
        },

        rsaEncrypt: function (value) {
            var publicKey = this.getServerPublicKey();
            var encrypt = new JSEncrypt();
            encrypt.setPublicKey(publicKey);
            var encrypted = encrypt.encrypt(value);
            return encrypted;
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
        var crypto = new JSEncrypt({ default_key_size: 2048 });
        crypto.getKey();
        clientPublicKey = crypto.getPublicKey();
        localStorage.setItem("SecureContextClientPrivateKey", crypto.getPrivateKey());
        localStorage.setItem("SecureContextClientPublicKey", clientPublicKey);
    }

    // Public functions
    return {
        challenge: function (base64data) {
            console.log("Challenge: ", base64data);
            base64data = base64data.substring(4);
            var decryptedChallenge = _.rsaDecrypt(base64data);
            console.log("Decrypted: ", decryptedChallenge);
            var encryptedChallenge = _.rsaEncrypt(decryptedChallenge);
            console.log("encryptedChallenge: ", encryptedChallenge);
            return encryptedChallenge;
        },

        decrypt: function (base64data) {
            var data = CryptoJS.enc.Base64.parse(base64data);
            var key = _.getSessionkey();
            var dataHex = CryptoJS.enc.Hex.stringify(data);
            var iv = CryptoJS.enc.Hex.parse(dataHex.slice(0, 32));
            var cipher = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Hex.parse(dataHex.slice(32)));
            var plain = CryptoJS.AES.decrypt(cipher, key, { iv: iv });
            return CryptoJS.enc.Utf8.stringify(plain);
        },

        encrypt: function (data) {
            var keyBytes = _.getSessionkey();
            var iv = CryptoJS.lib.WordArray.random(16);
            var encrypted = CryptoJS.AES.encrypt(data, keyBytes, { iv: iv });
            var encryptedHex = encrypted.iv.toString() + encrypted.ciphertext.toString();
            var encryptedBytes = CryptoJS.enc.Hex.parse(encryptedHex);
            return CryptoJS.enc.Base64.stringify(encryptedBytes);
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

        openChannel: function (serverPublicKey, encryptedKey) {
            _.setServerPublicKey(serverPublicKey);
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