
function preventBack() {
    window.history.forward();
}

function returnToPreviousPage() {
    window.history.back();
}

setTimeout("preventBack()", 0);
window.onunload = function () { null };

var name = document.getElementById("name").value;
var email = document.getElementById("email").value;
var password = document.getElementById("password").value;
var confirmPassword = document.getElementById("confirmPassword").value;
var passwordPattern = new RegExp("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[@$!%*?&])[A-Za-z\\d@$!%*?&]{8,}");

// "^" +                   //string starts here
// "(?=.*[0 - 9])" +       // at least 1 digit
// "(?=.* [A - Z])" +      // at least 1 uppercase
// "(?=.* [a - z])" +      // at least 1 lowercase
// "(?=.[!@#\$%\^&])" +    // at least 1 special character
// "(?=\\s+$)" +           // no white spaces
// "(?=.{4,})");           // at least 4 characters
//"(?=.* [^ A - Za - z0 - 9]).*");


function validateName() {
    if (document.forms['registerForm'].name.value === '') {
        alert("Vul je naam in");
        return false;
    }
    else {
        return true;
    }
}

function validateUsername() {
    if (document.forms['registerForm'].username.value === '') {
        alert("Vul een gebruikersnaam in");
        return false;
    }// else if met of die al bestaat
    else {
        return true;
    }
}

function validateEmail() {
    if (document.forms['registerForm'].email.value === '') {
        alert("Vul een email adres in");
        return false;
    }
    else {
        return true;
    }
}

function validatePassword() {
    if (document.forms['registerForm'].password.value == '') {
        alert("Vul een wachtwoord in");
        return false;
    }
    if (document.forms['registerForm'].password.value.match(passwordPattern) === null) {
        alert("juiste patroon gebruiken");
        return false;
    }
    else {
        return true;
    }
}

function confirmPassword() {
    if (document.forms['registerForm'].confirmPassword.value === '') {
        alert("Herhaal je wachtwoord");
        return false;
    }
    else if (document.forms['registerForm'].confirmPassword.value !== document.forms['registerForm'].password.value) {
        alert("Het wachtwoord komt niet overeen")
        return false;

    }
    else {
        return true;
    }
}

function validateAll() {
    if (!validateName() | !validateUsername() | !validateEmail() | !validatePassword() | !confirmPassword()) {
        returnToPreviousPage();
        return false;
    }
    else {
        window.location = "home_vp.html";
    }
}