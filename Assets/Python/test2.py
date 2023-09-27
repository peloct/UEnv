import threading, time
import traceback
import debugpy
import winsound as sd

"""

이벤트를 듣는 방식을 어떻게 하는가 하는 점.
이벤트 핸들러들은 여러 객체에 걸쳐있고, Unity 에서 온 이벤트의 ID 를 토대로 어떤 객체에 해당 이벤트가 전달될지 매핑이 있다.
그리고 각 객체에서 함수를 호출하면 이 Command 들이 중앙 버퍼로 쌓이는 형태


UnityObject
- 각종 커스텀 명령들
- onEvent

GlobalFunction
e.g.)
U.addButton(~~~, function)

"""


input_lock = threading.Lock()
input_buffer = []

output_lock = threading.Lock()
output_buffer = []


def push_output_buffer(obj):
    lock_acquired = False
    try:
        output_lock.acquire()
        lock_acquired = True
        output_buffer.append(obj)
        lock_acquired = False
        output_lock.release()
    except Exception as ex:
        if lock_acquired:
            output_lock.release()
            lock_acquired = False
        push_output_buffer(traceback.format_exc())


def input_thread():
    lock_acquired = False
    try:
        while True:
            input_value = input()
            input_lock.acquire()
            lock_acquired = True
            input_buffer.append(bytes(input_value, 'utf-8'))
            lock_acquired = False
            input_lock.release()
    except Exception as ex:
        if lock_acquired:
            input_lock.release()
            lock_acquired = False
        push_output_buffer(traceback.format_exc())


def main():
    debugpy.listen(('127.0.0.1', 5678))

    inputThread = threading.Thread(target=input_thread)
    inputThread.start()

    try:
        while True:
            buffer = []
            output_lock.acquire()
            for i in range(len(output_buffer)):
                buffer.append(output_buffer[i])
            output_buffer.clear()
            output_lock.release()

            input_lock.acquire()
            for i in range(len(input_buffer)):
                buffer.append(input_buffer[i])
            input_buffer.clear()
            input_lock.release()
            for i in buffer:
                print(b"\x00\x10".decode("utf-8") , flush=True)
            buffer.clear()
            time.sleep(0.001)
    except Exception as ex:
        print(traceback.format_exc())


if __name__ == '__main__':
    main()