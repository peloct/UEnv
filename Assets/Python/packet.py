import struct
import numpy as np


EXCEPTION_CODE = -1000
LOG_PACKET = -1001


class PacketFactory:
    generators = {}
    
    def add_generator(key_code, gen):
        PacketFactory.generators[key_code] = gen
    
    def get(key_code, data):
        if key_code not in PacketFactory.generators:
            return Packet(key_code, data)
        return PacketFactory.generators[key_code](key_code, data)


class Packet:
    def __init__(self, key_code, data) -> None:
        self.key_code = key_code
        self.data = data


def pack_data(*args, buffer=None):

    if buffer == None:
        buffer = bytearray()

    for data in args:
        if isinstance(data, bool):
            buffer.extend(struct.pack('<?', data))
        elif isinstance(data, int):
            buffer.extend(struct.pack('<i', data))
        elif isinstance(data, float):
            buffer.extend(struct.pack('<f', data))
        elif isinstance(data, str):
            data = data.encode('utf-8')
            buffer.extend(struct.pack('<i', len(data)))
            buffer.extend(data)
        elif isinstance(data, np.ndarray):
            shape = data.shape
            buffer.extend(struct.pack('<i', len(shape)))
            for dim in shape:
                buffer.extend(struct.pack('<i', dim))
            data = data.tobytes()
            buffer.extend(data)

    return buffer


def read_bool(byte_array):
    size = struct.calcsize('<?')
    ret = struct.unpack('<?', byte_array[:size])[0]
    return ret, byte_array[size:]


def read_int(byte_array):
    size = struct.calcsize('<i')
    ret = struct.unpack('<i', byte_array[:size])[0]
    return ret, byte_array[size:]


def read_float(byte_array):
    size = struct.calcsize('<f')
    ret = struct.unpack('<f', byte_array[:size])[0]
    return ret, byte_array[size:]


def read_str(byte_array):
    size = struct.calcsize('<i')
    length = struct.unpack('<i', byte_array[:size])[0]
    ret = byte_array[size:size+length].decode('utf-8')
    return ret, byte_array[size+length:]


def read_np(byte_array, dtype):
    int_size = struct.calcsize('<i')
    dim = struct.unpack('<i', byte_array[:int_size])[0]
    s = int_size
    shape = []
    for i in range(dim):
        shape.append(struct.unpack('<i', byte_array[s:s+int_size])[0])
        s = s + int_size
    byte_size = np.empty(shape=(1,), dtype=dtype).itemsize
    for i in shape:
        byte_size *= i
    ret = np.frombuffer(byte_array[s:s+byte_size], dtype=dtype)
    return ret, byte_array[s+byte_size:]


if __name__ == '__main__':
    print(pack_data('hello'))
    print(pack_data(1, 2, 3, np.array([1, 2, 3])))
    aa = pack_data(1, 2, 3, np.random.rand(2, 3, 4))
    print(aa)
    value, aa = read_int(aa)
    print(value)
    value, aa = read_int(aa)
    print(value)
    value, aa = read_int(aa)
    print(value)
    value, aa = read_np(aa, np.double)
    print(value)