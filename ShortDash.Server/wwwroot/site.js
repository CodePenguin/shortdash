function encrypt(publicKey) {
    console.log("Public key: ", publicKey);

    var text = document.getElementById("AccessTokenText");
    var message = text.value;
    console.log("Message: ", message);

    var encrypt = new JSEncrypt();
    encrypt.setPublicKey(publicKey);
    var encrypted = encrypt.encrypt(message);
    console.log("Encrypted: ", encrypted);
    return encrypted;
}