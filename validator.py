import re

def validateUser(user):
    return validateFirstName(user.firstname) and validateLastName(user.lastname) and validateUsername(user.username) and validateEmail(user.email) and validatePassword(user.password)

def validateFirstName(firstname):
    return len(firstname)>=2 and bool(re.search("^[a-zA-Z]+$",firstname))

def validateLastName(lastname):
    return len(lastname)>=2 and bool(re.search("^[a-zA-Z]+$",lastname))

def validateUsername(username):
    return len(username)>=5 and " "not in username

def validateEmail(email):
    return bool(re.search("^([A-Z|a-z][A-Za-z0-9]+)([\._]\w+)?@(\w+)(\.\w+)(\.\w+)?$",email))

def validatePassword(password):
    return len(password)>=8 and bool(re.search("^(?=.*\d)(?=.*[A-Za-z])[A-Za-z\d]+$",password))