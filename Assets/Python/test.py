import threading, time
import winsound as sd


lock = threading.Lock()
a = None


def readInput():
    global a
    while True:
        input_value = input()
        lock.acquire()
        a = input_value
        lock.release()


def write():
    try:
        while True:
            lock.acquire()
            if a is not None:
                print(a)
                a = None
                #sd.Beep(2000, 500)
            #if a is not None:
            #    print(a)
            #    a = None
            lock.release()
    except Exception as ex:
        sd.Beep(2000, 500)
        # print(ex)
        pass


def main():
    try:
        raise Exception('aaa')
    except Exception as ex:
        print(ex)
        pass


if __name__ == '__main__':
    inputThread = threading.Thread(target=readInput)
    inputThread.start()
    #writeThread = threading.Thread(target=write)
   # writeThread.start()
    main()

    inputThread.join()
   # writeThread.join()