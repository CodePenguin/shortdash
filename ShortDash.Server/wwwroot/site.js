function encrypt(publicKey) {
    console.log("Public key: ", publicKey);

    var text = document.getElementById("AccessTokenText");
    var message = text.value;
    console.log("Message: ", message);

    var crypto = new JSEncrypt();
    crypto.setPublicKey(publicKey);
    var encrypted = crypto.encrypt(message);
    console.log("Encrypted: ", encrypted);
    return encrypted;
}

function secureContextDecrypt(value) {
    var privateKey = getClientPrivateKey();
    console.log("Private key: ", privateKey);
    console.log("Value: ", value);
    var crypto = new JSEncrypt();
    crypto.setPrivateKey(privateKey);
    var encrypted = crypto.decrypt(value);
    console.log("Decrypted: ", encrypted);
    return encrypted;
}

function secureContextEncrypt(value) {
    var publicKey = getServerPublicKey();
    console.log("Public key: ", publicKey);
    console.log("Value: ", value);
    var encrypt = new JSEncrypt();
    encrypt.setPublicKey(publicKey);
    var encrypted = encrypt.encrypt(value);
    console.log("Encrypted: ", encrypted);
    return encrypted;
}

function setSecureInputTextValue(id, encryptedValue) {
    var privateKey = getPrivateKey();
    console.log("Public key: ", publicKey);
    console.log("Encrypted Value: ", encryptedValue);
    var text = document.getElementById(id);
    var encrypt = new JSEncrypt();
    encrypt.setPrivateKey(privateKey);
    var value = crypt.decrypt(encryptedValue);
    console.log("Decrypted Value: ", value);
    text.value = value;
}

function getClientPublicKey() {
    console.log("Reading client public key");
    return localStorage.getItem("SecureContextClientPublicKey");
}

function getClientPrivateKey() {
    console.log("Reading client private key");
    return localStorage.getItem("SecureContextClientPrivateKey");
}

function getServerPublicKey() {
    console.log("Reading server public key");
    return localStorage.getItem("SecureContextServerPublicKey");
}

function setServerPublicKey(publicKey) {
    console.log("Writing server public key");
    localStorage.setItem("SecureContextServerPublicKey", publicKey);
}

function initSecureContext(publicKey) {
    setServerPublicKey(publicKey);
    var clientPublicKey = getClientPublicKey();
    if (clientPublicKey == null) {
        console.log("Generating client keys...");
        var crypto = new JSEncrypt({ default_key_size: 2048 });
        crypto.getKey();
        clientPublicKey = crypto.getPublicKey();
        localStorage.setItem("SecureContextClientPrivateKey", crypto.getPrivateKey());
        localStorage.setItem("SecureContextClientPublicKey", clientPublicKey);
        console.log("Stored client keys.");
    }
    return clientPublicKey;
}

function initSecureContextSession(encryptedKey) {
    console.log("Received encrypted session key: ", encryptedKey);
    var crypto = new JSEncrypt();
    crypto.setPrivateKey(getClientPrivateKey());
    var key = crypto.decrypt(encryptedKey);
    localStorage.setItem("SecureContextSessionKey", key);

    var x = decrypt("ZcpYERcwAuGhevN3qgWQHlLtK5fpKokMRw961ZkE1yI=");
    console.log("Decrypted Value: ", x);
    var y = encrypt(x);
    console.log("Encrypted Again: ", y);
}

function decrypt(base64data) {
    var data = CryptoJS.enc.Base64.parse(base64data);
    var key = CryptoJS.enc.Base64.parse(localStorage.getItem("SecureContextSessionKey"));
    var dataHex = CryptoJS.enc.Hex.stringify(data);
    var iv = CryptoJS.enc.Hex.parse(dataHex.slice(0, 32));
    var cipher = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Hex.parse(dataHex.slice(32)));
    var plain = CryptoJS.AES.decrypt(cipher, key, { iv: iv });
    return CryptoJS.enc.Utf8.stringify(plain);
}

function encrypt(data) {
    var base64Key = localStorage.getItem("SecureContextSessionKey");
    var keyBytes = CryptoJS.enc.Base64.parse(base64Key);
    //var iv = CryptoJS.lib.WordArray.random(16);
    var iv = CryptoJS.enc.Hex.parse("65CA5811173002E1A17AF377AA05901E");
    var encrypted = CryptoJS.AES.encrypt(data, keyBytes, { iv: iv });
    window.encrypted = encrypted;
    var encryptedHex = encrypted.iv.toString() + encrypted.ciphertext.toString();
    var encryptedBytes = CryptoJS.enc.Hex.parse(encryptedHex);
    var encryptedBase64 = CryptoJS.enc.Base64.stringify(encryptedBytes);

    console.log("Key: ", encrypted.key.toString());
    console.log("IV: ", encrypted.iv.toString());
    console.log("Data: ", encryptedBase64);

    return encryptedBase64;
}

function initSecureInputText(id, dotnetHelper, initialValue) {
    console.log("initSecureInputText", id, dotnetHelper, initialValue);
    var inputText = document.getElementById(id);
    inputText.Text = secureContextDecrypt(initialValue);
    inputText.onchange = function () {
        dotnetHelper.invokeMethodAsync("OnClientChanged", secureContextEncrypt(inputText.value));
    }
}

function onChangeSecureInputText(sender, dotnetHelper) {
    console.log("onChangeSecureInputTextValue", sender, dotnetHelper);
    // dotnetHelper.invokeMethodAsync("OnClientChanged", secureContextEncrypt(sender.value));
}

/*
key = CryptoJS.enc.Hex.parse("03605B0BCDEA961F79E7464E74214E68E25A05E691E2B6FEC46C88061CEAFCB9");
data = CryptoJS.enc.Base64.parse("ZcpYERcwAuGhevN3qgWQHlLtK5fpKokMRw961ZkE1yI=");
dataHex = CryptoJS.enc.Hex.stringify(data);
iv = CryptoJS.enc.Hex.parse(dataHex.slice(0, 32));
cipher = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Hex.parse(dataHex.slice(32)));
plain = CryptoJS.AES.decrypt(cipher, key, { iv: iv });
console.log("Plain: ", CryptoJS.enc.Utf8.stringify(plain));
*/