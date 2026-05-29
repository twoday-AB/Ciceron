'use strict';

window.addEventListener('load', function() {
    var forms = document.getElementsByClassName('needs-validation');

    var validation = Array.prototype.filter.call(forms, function(form) {
        form.addEventListener('submit', function(event) {
        if (form.checkValidity() === false) {
            event.preventDefault();
            event.stopPropagation();
        }
        form.classList.add('was-validated');
        }, false);
    });

    checkMessageTypeOrTTL();

    document.getElementById("message_type").addEventListener("change", (e) => checkMessageTypeOrTTL());
    document.getElementById("ttl").addEventListener("change", (e) => checkMessageTypeOrTTL());
}, false);

function checkMessageTypeOrTTL() {
    const messageTypeE = document.getElementById("message_type");
    const ttlE = document.getElementById("ttl");

    const messageTypeValue = messageTypeE.value;
    const ttlValue = ttlE.value;

    const isRequired = !(messageTypeValue || ttlValue)
    messageTypeE.required = ttlE.required = isRequired;

    const messageTypeLabel = document.querySelector("label[for=message_type]");
    const ttlLabel = document.querySelector("label[for=ttl]");
    if (isRequired) {
        messageTypeLabel.classList.add("required")
        ttlLabel.classList.add("required")
    } else {
        messageTypeLabel.classList.remove("required")
        ttlLabel.classList.remove("required")
    }
}

function onTypeChange(index) {
    const typeElement = document.getElementById("type-" + index)

    const nameElement = document.getElementById("name-" + index);
    const ssnElement = document.getElementById("ssn-" + index);
    const emailElement = document.getElementById("mail-" + index);

    const nameLabel = document.querySelector("label[for=name-" + index + "]");
    const ssnLabel = document.querySelector("label[for=ssn-" + index + "]");
    const emailLabel = document.querySelector("label[for=mail-" + index + "]");

    if (typeElement.value == "external") {
        emailElement.required = true
        nameElement.required = true
        ssnElement.required = false

        emailLabel.classList.add("required");
        nameLabel.classList.add("required");
        ssnLabel.classList.remove("required");
    }
    else {
        emailElement.required = false
        nameElement.required = false
        ssnElement.required = true

        emailLabel.classList.remove("required");
        nameLabel.classList.remove("required");
        ssnLabel.classList.add("required");
    }
}
