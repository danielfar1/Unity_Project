from fastapi import FastAPI
from pydantic import BaseModel
from restapi.db import checkusername,checkemail,createUser,checkLogin
from restapi.validator import validateUser

app = FastAPI()

class User(BaseModel):
    username: str
    password: str
    email: str
    firstname: str
    lastname: str

@app.get("/")
async def root():
    return {"message": "Welcome to the REST API"}


@app.get("/user_username")
async def read_item(username:str):
    userExist = checkusername(username)
    return userExist

@app.get("/user_email")
async def read_item(email:str):
    emailExist = checkemail(email)
    return emailExist

@app.get("/user_login/")
async def read_item(password: str,email:str):
    loginAnswer = checkLogin(password,email)
    return loginAnswer


@app.post("/create_user")
async def create_item(user: User):
    createUser(user)
    return {"status": "Success"}

@app.post("/validate")
async def create_item(validate: User):
    result=validateUser(validate)
    return {"status": result}

