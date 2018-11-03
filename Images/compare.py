from PIL import Image
import numpy as np


def compImages(file, window):
    naive = Image.open(r"D:\Users\jcoo092\Writing\2018\IVCNZ18\ivcnz2018\Images\Outputs\Naive\\" +
                       f + "_naive_" + str(window) + ".png")
    braunl = Image.open(r"D:\Users\jcoo092\Writing\2018\IVCNZ18\ivcnz2018\Images\Outputs\Braunl\\" +
                        f + "_braunl_" + str(window) + ".png")
    cml = Image.open(r"D:\Users\jcoo092\Writing\2018\IVCNZ18\ivcnz2018\Images\Outputs\CML\\" +
                     f + "_cml_" + str(window) + ".png")

    narr = np.array(naive)
    barr = np.array(braunl)
    carr = np.array(cml)

    if np.array_equal(narr, barr):
        print(
            "image {0} window {1} - naive & braunl matched".format(file, window))

    if np.array_equal(narr, carr):
        print("image {0} window {1} - naive & cml matched".format(file, window))

    if np.array_equal(barr, carr):
        print("image {0} window {1} - braunl & cml matched".format(file, window))


files = ["very small", "small", "peppers_gray", "medium"]
windows = [3, 5, 7, 9, 11]

for f in files:
    for w in windows:
        compImages(f, w)


# def compImages(windowSize):
#     baseIm = Image.open(
#         r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Inputs\peppers_gray.png")
#     naiveIm = Image.open(
#         r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Outputs\naive_peppers_gray_noisy_" + windowSize + ".png")
#     braunlIm = Image.open(
#         r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Outputs\braunl_peppers_gray_noisy_" + windowSize + ".png")
#     cmlIm = Image.open(
#         r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Outputs\cml_peppers_gray_noisy_" + windowSize + ".png")

#     numPixels = 512 * 512 * 3

#     baseArr = np.array(baseIm)
#     naiveArr = np.array(naiveIm)
#     braunlArr = np.array(braunlIm)
#     cmlArr = np.array(cmlIm)

#     print(np.array_equal(naiveArr, braunlArr))
#     print(np.array_equal(braunlArr, cmlArr))
#     print(np.array_equal(naiveArr, cmlArr))

#     naiveDiff = np.abs(baseArr - naiveArr)
#     braunlDiff = np.abs(baseArr - braunlArr)
#     cmlDiff = np.abs(baseArr - cmlArr)

#     nds = np.sum(naiveDiff)
#     bds = np.sum(braunlDiff)
#     cds = np.sum(cmlDiff)

#     print("window size: " + windowSize)
#     print(nds / numPixels)
#     print(bds / numPixels)
#     print(cds / numPixels)

# for i in [3, 5, 7]:
#     compImages(str(i))
