from PIL import Image
#import numpy as np
import random


""" def maybeMakeNoise(pixel):
    print(pixel)
    rand = random.randint(1, 100)
    if rand > 80:
        if pixel > 127:
            return (255, 255, 255)
        else:
            return (0, 0, 0) """

basepath = r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Inputs"

filenames = ["very small", "small",
             "medium", "big", "very big"]

for fn in filenames:

    im = Image.open(
        basepath + "\\" + fn + ".jpeg")
    # print(im.mode)

    noisyIm = im.copy()

    for x in range(noisyIm.width):
        for y in range(noisyIm.height):
            if random.randint(1, 100) > 94:
                (r, g, b) = noisyIm.getpixel((x, y))
                """ if r > 127:
                    n = (0, 0, 0)
                else:
                    n = (255, 255, 255) """

                if random.randint(1, 2) > 1:
                    n = (0, 0, 0)
                else:
                    n = (255, 255, 255)
                noisyIm.putpixel((x, y), n)

    """ j = [noisyIm.getpixel((x, y)) for x in range(20) for y in range(20)]
    print(*j) """

    im.save(basepath + "\\" + fn + ".png")

    noisyIm.save(
        basepath + "\\" + fn + "_noisy.png")
