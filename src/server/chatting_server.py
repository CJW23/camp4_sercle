import socket
from _thread import *
import marshal

HOST = '127.0.0.1'
PORT = 3098
#소켓 서버 설정
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
server_socket.setsockopt(socket.IPPROTO_TCP, socket.TCP_NODELAY, 1)
server_socket.bind((HOST, PORT))
server_socket.listen()

#서버에 접속한 유저 소켓 리스트 저장
list_of_clients = []
print('Lobby server start')


#각 클라이언트 소켓 쓰레드
def client_thread(user_socket):
    while True:
        try:
            #클라이언트에서 온 데이터 수신
            message = user_socket[0].recv(1024)
            if not message:
                remove(user_socket)
                break
            #print(str(str(user_socket[2].decode()) + ":").encode() + message)
            #클라이언트에서 온 데이터를 다른 클라이언트에게도 전달
            broadcast(message, user_socket)
        except Exception as e:
            print(e)
            continue


def broadcast(message, user_socket):
    #print("a : " + str("awdasdawd".encode()))
    print(int.from_bytes(message[4:8], byteorder='big'))
    for clients in list_of_clients:

        try:
            if clients[1][1] != user_socket[1][1]:
                clients[0].send(message)
            #clients[0].send(str(str(user_socket[2].decode()) + ":").encode() + message)
        except:
            clients[0].close()
            remove(clients)


def remove(connection):
    if connection in list_of_clients:
        list_of_clients.remove(connection)
        print(connection[2].decode() + "님이 나가셨습니다.")


while True:
    print('waiting new client..')
    client_socket, addr = server_socket.accept()        #소켓
    print(addr[0] + ' ' + str(addr[1]) + ' connected')
    user_email = client_socket.recv(1024)

    list_of_clients.append([client_socket, addr, user_email])
    #쓰레드 시작
    start_new_thread(client_thread, (list_of_clients[-1],))

server_socket.close()