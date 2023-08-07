with open('rx_1315098092.txt') as f:
    content = f.read()
data = content.split(",")
#print(data)
data.pop()
import numpy as np
data = np.array(data)
x_l = 540
y_l = 960
data = data.reshape((x_l, y_l))
#print(data.dtype)
rdata = np.zeros((x_l, y_l, 3))
for x in range(x_l):
 for y in range(y_l):
    data[x,y] = data[x,y].lstrip('#')
    #if data[x, y] != '000000':
       #print('het')
    rdata[x,y] = tuple(int(data[x, y][i:i+2], 16) for i in (0, 2, 4))
#print(rdata.shape)
from PIL import Image
image =Image.fromarray(rdata.astype(np.uint8))
image = image.resize((1920, 1080))
image.show()