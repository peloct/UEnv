from packet import *

class P2UTest(Packet):
    def __init__(self, text:str, ndarray_data:np.ndarray, int_data:int, float_data:float):
        super().__init__(0, pack_data(text, ndarray_data, int_data, float_data))
        assert ndarray_data.dtype == np.float32


class U2PTest(Packet):
    KEY = 1
    def __init__(self, key_code, data):
        super().__init__(key_code, data)
        data = self.data
        self.text, data = read_str(data)

def init_factory():
    PacketFactory.add_generator(1, U2PTest)

