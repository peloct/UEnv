import debugpy
from uenv import UEnv
from packet_factory import *


env = UEnv()


def main():
    debugpy.listen(('localhost', 5678))
    env.send_packet(P2UTest("P2UTest", np.array([[1, 2], [3, 4]], dtype=np.float32), 5, 6.0))

    def on_u2p(packet):
        if isinstance(packet, U2PTest):
            env.log(packet.text)

    env.register_packet_handler(U2PTest.KEY, on_u2p)
    env.loop()


if __name__ == '__main__':
    main()
