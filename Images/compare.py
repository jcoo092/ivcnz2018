from PIL import Image
import numpy as np


def compImages(windowSize):
    baseIm = Image.open(
        r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Inputs\peppers_gray.png")
    naiveIm = Image.open(
        r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Outputs\naive_peppers_gray_noisy_" + windowSize + ".png")
    braunlIm = Image.open(
        r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Outputs\braunl_peppers_gray_noisy_" + windowSize + ".png")
    naiveIm = Image.open(
        r"D:\Users\jcoo092\Writing\2018\IVCNZ18\Images\Outputs\cml_peppers_gray_noisy_" + windowSize + ".png")

    numPixels = 512 * 512 * 3

    baseArr = np.array(baseIm)
    naiveArr = np.array(naiveIm)
    braunlArr = np.array(braunlIm)
    cmlArr = np.array(naiveIm)

    print(np.array_equal(naiveArr, braunlArr))
    print(np.array_equal(braunlArr, cmlArr))
    print(np.array_equal(naiveArr, cmlArr))

    naiveDiff = np.abs(baseArr - naiveArr)
    braunlDiff = np.abs(baseArr - braunlArr)
    cmlDiff = np.abs(baseArr - cmlArr)

    nds = np.sum(naiveDiff)
    bds = np.sum(braunlDiff)
    cds = np.sum(cmlDiff)

    print("window size: " + windowSize)
    print(nds / numPixels)
    print(bds / numPixels)
    print(cds / numPixels)


for i in [3, 5, 7]:
    compImages(str(i))
