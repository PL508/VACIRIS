from deepface import DeepFace
from deepface.basemodels import ArcFace
import os
import time
from datetime import datetime

start_time = datetime.now()
print("SCANNING...");

file = open("collect.txt", "w+");

model = ArcFace.loadModel()

for filename in os.listdir("rtdata"):
    try:
        df = DeepFace.find(img_path = os.path.join("rtdata", filename), db_path = "database", model_name = "ArcFace",
                        model = model, distance_metric = 'euclidean_l2', detector_backend = 'opencv');
        if df.shape[0] > 0:
            matched = df.iloc[0].identity
            file.write(os.path.dirname(matched).replace("images/", "") + "\n");
    except:
        file.write("null\n");

file.close();
end_time = datetime.now()

print("COMPLETED!")
print('Duration: {}'.format(end_time - start_time))

time.sleep(2);

