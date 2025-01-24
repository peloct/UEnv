import threading, time
import traceback
import base64
from packet import *
from packet_factory import init_factory


class UEnv:

    def __init__(self, run_by_unity=True):
        UEnv.env = self

        self.input_lock = threading.Lock()
        self.input_buffer = []

        self.output_lock = threading.Lock()
        self.output_buffer = []

        self.packet_handler_dic = {}
        self.run_by_unity = run_by_unity
        init_factory()


    def register_packet_handler(self, key_code, handler):
        if key_code not in self.packet_handler_dic:
            self.packet_handler_dic[key_code] = []
        self.packet_handler_dic[key_code].append(handler)


    def unregister_packet_handler(self, key_code, handler):
        self.packet_handler_dic[key_code].remove(handler)


    def log(self, text):
        if not isinstance(text, str):
            text = str(text)
        if self.run_by_unity:
            print(f'!{base64.b64encode(text.encode("utf-8")).decode("ascii")}')
        else:
            print(text)


    def log_warning(self, text):
        if not isinstance(text, str):
            text = str(text)
        if self.run_by_unity:
            print(f'!!{base64.b64encode(text.encode("utf-8")).decode("ascii")}')
        else:
            print(text)


    def log_error(self, text):
        if not isinstance(text, str):
            text = str(text)
        if self.run_by_unity:
            print(f'!!!{base64.b64encode(text.encode("utf-8")).decode("ascii")}')
        else:
            print(text)


    def send_packet(self, packet):
        if not self.run_by_unity:
            return
        
        try:
            assert isinstance(packet, Packet)
            meta_byte = bytearray()
            pack_data(packet.key_code, buffer=meta_byte)
            data_byte = packet.data
            pack_data(len(data_byte), buffer=meta_byte)
            byte = meta_byte + data_byte
            self.push_output_buffer(byte)
        except Exception as ex:
            self.log_error(traceback.format_exc())


    def send_data_as_packet(self, key_code, *args):
        meta_byte = bytearray()
        pack_data(key_code, buffer=meta_byte)
        data_byte = pack_data(*args)
        pack_data(len(data_byte), buffer=meta_byte)
        byte = meta_byte + data_byte
        self.push_output_buffer(byte)


    def push_output_buffer(self, obj):
        lock_acquired = False
        try:
            self.output_lock.acquire()
            lock_acquired = True
            self.output_buffer.append(obj)
            lock_acquired = False
            self.output_lock.release()
        except Exception as ex:
            if lock_acquired:
                self.output_lock.release()
                lock_acquired = False
            self.log_error(traceback.format_exc())


    def input_thread():
        lock_acquired = False
        try:
            while True:
                input_value = input()
                UEnv.env.input_lock.acquire()
                lock_acquired = True
                UEnv.env.input_buffer.append(input_value)
                lock_acquired = False
                UEnv.env.input_lock.release()
        except Exception as ex:
            if lock_acquired:
                UEnv.env.input_lock.release()
                lock_acquired = False
            UEnv.env.log_error(traceback.format_exc())


    def loop(self):

        inputThread = threading.Thread(target=UEnv.input_thread)
        inputThread.start()

        is_reading_packet = False
        packet_key = -1
        packet_size = 987654321
        buffer = bytearray()
        new_packets = []

        try:
            while True:
                output_buffer_proxy = []
                self.output_lock.acquire()
                for i in range(len(self.output_buffer)):
                    output_buffer_proxy.append(self.output_buffer[i])
                self.output_buffer.clear()
                self.output_lock.release()

                input_buffer_proxy = []
                self.input_lock.acquire()
                for i in range(len(self.input_buffer)):
                    input_buffer_proxy.append(self.input_buffer[i])
                self.input_buffer.clear()
                self.input_lock.release()
    
                output = bytearray()
                for i in range(len(output_buffer_proxy)):
                    output.extend(output_buffer_proxy[i])
                print(base64.b64encode(output).decode('ascii'), flush=True)
    
                for i in range(len(input_buffer_proxy)):
                    byte_data = base64.b64decode(bytes(input_buffer_proxy[i], 'ascii'))
                    buffer.extend(byte_data)
                
                if len(buffer) > 0:
                    while (not is_reading_packet and len(buffer) >= 8) or packet_size <= len(buffer):
                        if not is_reading_packet:
                            is_reading_packet = True
                            packet_key, buffer = read_int(buffer)
                            packet_size, buffer = read_int(buffer)
                        
                        if packet_size <= len(buffer):
                            data = buffer[:packet_size]
                            buffer = buffer[packet_size:]
                            new_packets.append(PacketFactory.get(packet_key, data))
                            is_reading_packet = False
                            packet_size = 987654321
    
                for each_packet in new_packets:
                    if each_packet.key_code in self.packet_handler_dic:
                        handlers = self.packet_handler_dic[each_packet.key_code]
                        try:
                            for each_handler in handlers:
                                each_handler(each_packet)
                        except Exception as ex:
                            self.log_error(traceback.format_exc())
                
                new_packets.clear()
                time.sleep(0.001)
        except Exception as ex:
            self.log_error(traceback.format_exc())
