import sqlite3
from contextlib import closing
import os
from restapi.validator import validateUser

DB_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "../1vs1Football.db")
def checkusername(username):
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute(f'select Username from users where Username="{username}"')
            if cursor.fetchone() is not None:
                return True
            return False
        
def checkemail(email):
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute(f'select Email from users where Email="{email}"')
            if cursor.fetchone() is not None:
                return True
            return False
        
def checkLogin(password,email):
    with closing(sqlite3.connect(DB_FILE)) as conn:
        with conn:
            cursor = conn.cursor()
            cursor.execute(f'SELECT email,password FROM users WHERE email="{email}"AND password="{password}"')
            if cursor.fetchone() is not None:
                return True
            return False
        
def createUser(user):
    if( validateUser(user)):
        print(validateUser(user))
        with closing(sqlite3.connect(DB_FILE)) as conn:
            with conn:
                data = [(None,user.username,user.password,user.email,user.firstname,user.lastname)]
                cursor = conn.cursor()
                cursor.executemany("INSERT INTO users VALUES(?,?,?,?,?,?)",data)
                return True
        

