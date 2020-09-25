var secureContext = (function () {
    // Internal functions
    var _ = {
        getClientPublicKey: function () {
            console.log("Reading client public key");
            return localStorage.getItem("SecureContextClientPublicKey");
        },

        getClientPrivateKey: function () {
            console.log("Reading client private key");
            return localStorage.getItem("SecureContextClientPrivateKey");
        },

        getServerPublicKey: function () {
            console.log("Reading server public key");
            return localStorage.getItem("SecureContextServerPublicKey");
        },

        getSessionkey: function () {
            var key = localStorage.getItem("SecureContextSessionKey");
            return CryptoJS.enc.Base64.parse(key);
        },

        rsaDecrypt: function (value) {
            var privateKey = this.getClientPrivateKey();
            console.log("Private key: ", privateKey);
            console.log("Value: ", value);
            var crypto = new JSEncrypt();
            crypto.setPrivateKey(privateKey);
            var encrypted = crypto.decrypt(value);
            console.log("Decrypted: ", encrypted);
            return encrypted;
        },

        rsaEncrypt: function (value) {
            var publicKey = this.getServerPublicKey();
            console.log("Public key: ", publicKey);
            console.log("Value: ", value);
            var encrypt = new JSEncrypt();
            encrypt.setPublicKey(publicKey);
            var encrypted = encrypt.encrypt(value);
            console.log("Encrypted: ", encrypted);
            return encrypted;
        },

        setSessionKey: function (encryptedKey) {
            console.log("Received encrypted session key: ", encryptedKey);
            var key = this.rsaDecrypt(encryptedKey);
            localStorage.setItem("SecureContextSessionKey", key);
        },

        setServerPublicKey: function (publicKey) {
            console.log("Writing server public key");
            localStorage.setItem("SecureContextServerPublicKey", publicKey);
        },
    };

    // Initialize context
    if (_.getClientPublicKey() === null) {
        console.log("Generating client keys...");
        var crypto = new JSEncrypt({ default_key_size: 2048 });
        crypto.getKey();
        clientPublicKey = crypto.getPublicKey();
        localStorage.setItem("SecureContextClientPrivateKey", crypto.getPrivateKey());
        localStorage.setItem("SecureContextClientPublicKey", clientPublicKey);
        console.log("Stored client keys.");
    }

    return {
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
            window.encrypted = encrypted;
            var encryptedHex = encrypted.iv.toString() + encrypted.ciphertext.toString();
            var encryptedBytes = CryptoJS.enc.Hex.parse(encryptedHex);
            var encryptedBase64 = CryptoJS.enc.Base64.stringify(encryptedBytes);

            console.log("Key: ", encrypted.key.toString());
            console.log("IV: ", encrypted.iv.toString());
            console.log("Data: ", encryptedBase64);

            return encryptedBase64;
        },

        exportPublicKey: function () {
            return _.getClientPublicKey();
        },

        initSecureInputText: function (id, dotnetHelper, initialValue) {
            console.log("initSecureInputText", id, dotnetHelper, initialValue);
            var inputText = document.getElementById(id);
            inputText.Text = _.rsaDecrypt(initialValue);
            inputText.onchange = function () {
                dotnetHelper.invokeMethodAsync("OnClientChanged", secureContextEncrypt(inputText.value));
            }
        },

        onChangeSecureInputText: function (sender, dotnetHelper) {
            console.log("onChangeSecureInputTextValue", sender, dotnetHelper);
            // dotnetHelper.invokeMethodAsync("OnClientChanged", secureContextEncrypt(sender.value));
        },

        openChannel: function (serverPublicKey, encryptedKey) {
            _.setServerPublicKey(serverPublicKey);
            _.setSessionKey(encryptedKey);
        },

        setSecureInputTextValue: function (id, encryptedValue) {
            var privateKey = _.getClientPrivateKey();
            console.log("Public key: ", privateKey);
            console.log("Encrypted Value: ", encryptedValue);
            var text = document.getElementById(id);
            var encrypt = new JSEncrypt();
            encrypt.setPrivateKey(privateKey);
            var value = crypt.decrypt(encryptedValue);
            console.log("Decrypted Value: ", value);
            text.value = value;
        },

        XXXencryptXXX: function (publicKey) {
            console.log("Public key: ", publicKey);

            var text = document.getElementById("AccessTokenText");
            var message = text.value;
            console.log("Message: ", message);

            var crypto = new JSEncrypt();
            crypto.setPublicKey(publicKey);
            var encrypted = crypto.encrypt(message);
            console.log("Encrypted: ", encrypted);
            return encrypted;
        },
    };
})();
window.secureContext = secureContext;