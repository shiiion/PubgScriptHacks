#an updated version of dump_to_simple_cpp.py
import numpy as np
np.random.seed(1337)
from keras.models import Sequential, model_from_json
import json
import argparse

np.set_printoptions(threshold=np.inf)
parser = argparse.ArgumentParser(description='This is a simple script to dump Keras model into simple format suitable for porting into pure C++ model')

parser.add_argument('-a', '--architecture', help="JSON with model architecture", required=True)
parser.add_argument('-w', '--weights', help="Model weights in HDF5 format", required=True)
parser.add_argument('-o', '--output', help="Ouput file name", required=True)
parser.add_argument('-v', '--verbose', help="Verbose", required=False)
args = parser.parse_args()

print('Read architecture from', args.architecture)
print('Read weights from', args.weights)
print('Writing to', args.output)

arch = open(args.architecture).read()
model = model_from_json(arch)
model.load_weights(args.weights)
model.compile(loss='categorical_crossentropy', optimizer='adadelta')
arch = json.loads(arch)

with open(args.output, 'w', newline='') as fout:
	for ind, l in enumerate(arch["config"]):
		if l['class_name'] == 'Conv2D':
			fout.write('layer:Conv2D\n')
			fout.write('activation:' + l['config']['activation'] + '\n')
			W = model.layers[ind].get_weights()[0]
			fout.write(str(W.shape[0]) + ',' + str(W.shape[1]) + ',' + str(W.shape[2]) + ',' + str(W.shape[3]) + '\n')
			for i in range(W.shape[0]):
				for j in range(W.shape[1]):
					for k in range(W.shape[2]):
						fout.write(str(W[i,j,k]).replace('\n', '') + '\n')
			fout.write('bias:' + str(model.layers[ind].get_weights()[1]).replace('\n', '') + '\n')
		if l['class_name'] == 'MaxPooling2D':
			fout.write('layer:MaxPooling2D\n')
			fout.write(str(l['config']['pool_size'][0]) + ',' + str(l['config']['pool_size'][1]) + '\n')
		if l['class_name'] == 'Dense':
			fout.write('layer:Dense\n')
			fout.write('activation:' + l['config']['activation'] + '\n')
			W = model.layers[ind].get_weights()[0]
			fout.write(str(W.shape[0]) + ',' + str(W.shape[1]) + '\n')
			for w in W:
				fout.write(str(w).replace('\n', '') + '\n')
			fout.write('bias:' + str(model.layers[ind].get_weights()[1]).replace('\n', '') + '\n')
		if l['class_name'] == 'Flatten':
			fout.write('layer:Flatten\n')