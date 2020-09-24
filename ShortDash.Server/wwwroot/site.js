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