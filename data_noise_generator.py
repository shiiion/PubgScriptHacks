#run this script to generate 5 modified versions for each image in the training set
from keras.preprocessing.image import ImageDataGenerator, array_to_img, img_to_array, load_img
from os import listdir
from os.path import isfile, join
from os import walk

for directory in walk('screendata/png/train'):
	print('dumping directory ' + directory[0])
	for f in directory[2]:
		datagen = ImageDataGenerator(
		width_shift_range=0.05,
		height_shift_range=0.05,
		zoom_range=0.05,
		fill_mode='nearest')

		img = load_img(directory[0] + '\\' + f)  # this is a PIL image
		x = img_to_array(img)  # this is a Numpy array with shape (3, 150, 150)
		x = x.reshape((1,) + x.shape)  # this is a Numpy array with shape (1, 3, 150, 150)
		i = 0
		for batch in datagen.flow(x, batch_size=1,
								  save_to_dir=directory[0], save_prefix=f[2] + str(i), save_format='png'):
			i += 1
			if i > 5:
				break