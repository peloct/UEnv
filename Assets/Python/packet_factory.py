from packet import *

class AddButton(Packet):
    def __init__(self, id:str):
        super().__init__(0, pack_data(id))

class UIEvent(Packet):
    KEY = 1
    def __init__(self, key_code, data):
        super().__init__(key_code, data)
        data = self.data
        self.clicked_button, data = read_str(data)
        self.key, data = read_int(data)
        

def init_factory():
    PacketFactory.add_generator(1, UIEvent)

