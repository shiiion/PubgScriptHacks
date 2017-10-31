#trains model on 24 different classes, pretty accurate

from __future__ import print_function
import tensorflow as tf
import keras
from keras.backend.tensorflow_backend import set_session
from keras.datasets import mnist
from keras.models import Sequential
from keras.layers import Dense, Dropout, Flatten
from keras.layers import Conv2D, MaxPooling2D
from keras import backend as K
from keras.callbacks import EarlyStopping, ModelCheckpoint, ReduceLROnPlateau
from keras.preprocessing.image import ImageDataGenerator, array_to_img, img_to_array, load_img

config = tf.ConfigProto()
config.gpu_options.allow_growth = True
set_session(tf.Session(config=config))

batch_size = 128
num_classes = 24
epochs = 12

# input image dimensions
img_rows, img_cols = 64, 64

train_datagen = ImageDataGenerator()
train_generator = train_datagen.flow_from_directory(
        'screendata/png - Copy (2)/train',  # this is the target directory
        target_size=(img_rows, img_cols),
        batch_size=batch_size,
        class_mode='categorical',
        color_mode='grayscale',
        shuffle=True
        )  # since we use binary_crossentropy loss, we need binary labels
val_datagen = ImageDataGenerator()
val_generator = val_datagen.flow_from_directory(
        'screendata/png - Copy (2)/val',  # this is the target directory
        target_size=(img_rows, img_cols),
        batch_size=batch_size,
        class_mode='categorical',
        color_mode='grayscale',
        shuffle=True
        )  # since we use binary_crossentropy loss, we need binary labels
print(train_generator)

input_shape = (img_rows, img_cols, 1)

model = Sequential()
model.add(Conv2D(32, kernel_size=(3, 3),
                 activation='relu',
                 input_shape=input_shape))
print(model.layers[-1].output_shape) 
model.add(Conv2D(32, kernel_size=(3, 3), activation='relu'))
print(model.layers[-1].output_shape) 
model.add(MaxPooling2D(pool_size=(2, 2)))
print(model.layers[-1].output_shape)
model.add(Dropout(0.25))
model.add(Flatten())
print(model.layers[-1].output_shape)
model.add(Dense(128, activation='relu'))
print(model.layers[-1].output_shape)
model.add(Dropout(0.5))
model.add(Dense(24, activation='softmax'))
print(model.layers[-1].output_shape)

model.compile(loss=keras.losses.categorical_crossentropy,
              optimizer=keras.optimizers.Adadelta(),
              metrics=['accuracy'])

weights_path = 'modelweights.h5'
model_checkpoint = ModelCheckpoint(weights_path, monitor='val_acc', verbose=1, save_best_only=True, mode='max')
early_stopping = EarlyStopping(monitor='val_acc', patience=100, verbose=0, mode='auto')
reduce_lr = ReduceLROnPlateau(monitor='val_loss', factor=0.2, patience=5, min_lr=0)
callbacks_list = [model_checkpoint, early_stopping, reduce_lr]


model.fit_generator(
        train_generator,
        steps_per_epoch=2000 // batch_size,
        epochs=200,
        validation_data=val_generator,
        validation_steps=2000 // batch_size,
        callbacks=callbacks_list)

json_string = model.to_json()
text_file = open('modelarchsmall.json', 'w')
text_file.write(json_string)
text_file.close()

model.save_weights('modelweightssmall.h5')