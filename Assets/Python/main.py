import debugpy
from uenv import UEnv
from packet_factory import AddButton, UIEvent


env = UEnv()


def main():
    debugpy.listen(('localhost', 5678))
    env.send_packet(AddButton("heelo"))

    def on_ui_vent(packet):
        if isinstance(packet, UIEvent):
            env.log(packet.clicked_button)

    env.register_packet_handler(UIEvent.KEY, on_ui_vent)
    env.loop()


if __name__ == '__main__':
    main()
