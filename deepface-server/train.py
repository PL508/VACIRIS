from deepface import DeepFace
from deepface.basemodels import ArcFace
from zipfile import ZipFile
import firebase_admin
from firebase_admin import credentials
from firebase_admin import storage
import pyrebase
import shutil
import json
import os

if os.path.exists("images") == True:
	shutil.rmtree("images");

firebaseConfig = {
	"apiKey": "AIzaSyDC4byKw4OegfZpLUu3o-6OanvcTJmeeoM",
 	"authDomain": "vaciris-4a6d4.firebaseapp.com",
 	"databaseURL": "https://vaciris-4a6d4-default-rtdb.asia-southeast1.firebasedatabase.app",
 	"projectId": "vaciris-4a6d4",
 	"storageBucket": "vaciris-4a6d4.appspot.com",
 	"serviceAccount": "serviceAccountKey.json"
}

firebase = pyrebase.initialize_app(firebaseConfig)
rtd = firebase.storage()

rtd.child("images.zip").download("images.zip");

with ZipFile("images.zip", 'r') as zip:
    zip.extractall();

os.remove("images.zip");

model = ArcFace.loadModel()

df = DeepFace.find(img_path = "example.png", db_path = "images", model_name = "ArcFace", 
	model = model, distance_metric = 'euclidean_l2', detector_backend = 'mtcnn', enforce_detection = False);

cred = credentials.Certificate("serviceAccountKey.json")
default_app = firebase_admin.initialize_app(cred, {'storageBucket': 'vaciris-4a6d4.appspot.com'})

bucket = storage.bucket()
blob = bucket.blob('representations_arcface.pkl')

blob.metadata = {"firebaseStorageDownloadTokens": '7af17a1e-ed31-4ed1-b071-877d8ddfbb55'}
blob.upload_from_filename(filename = 'images/representations_arcface.pkl', content_type = 'representations_arcface.pkl');

print("DONE");
