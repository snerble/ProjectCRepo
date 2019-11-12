//type = "text/javascript" >
//    function preventBack() {
//        window.history.forward();
//    }

//setTimeout("preventBack()", 0);
//window.onunload = function () { null };

var username = document.getElementById("username").value;
var password = document.getElementById("password").value;
var emailPattern = /\S+@\S+\.\S+/;

function validateUsername() {
    if (document.forms['loginform'].username.value !== "Daphne") {
        alert("De gebruikersnaam is onjuist")
        return false;
    }
    else {
        return true
    }
}

function validatePassword() {
    if (document.forms['loginform'].password.value !== "1") {
        alert("Wachtwoord is onjuist")
        return false;
    }
    else {
        return true;
    }
}

function validateAll() {
    alert("heyo")
    if (!validateUsername() | !validatePassword()) {
        return false
    }
    else {
        window.location = "Login1"
    }
}