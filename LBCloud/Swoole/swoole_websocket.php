<?php
/**
 * websocket
 */
$server = new swoole_websocket_server("0.0.0.0", 9501);

$server->on('open', function (swoole_websocket_server $server, $request) {
    echo "server: handshake success with fd{$request->fd}\n";
});

$server->on('message', function (swoole_websocket_server $server, $frame) {
    /*echo "receive from {$frame->fd}:{$frame->data},opcode:{$frame->opcode},fin:{$frame->finish}\n";
    $server->push($frame->fd, "this is server");*/
   	$data = json_decode($frame->data, true);
   	switch($data['Act']){
   		case 'bind':
			$player_id = $data['Id'];
			$player_key = $data['Key'];
			$player_mac = $data['Mac'];
			$redis_key = md5("{$player_id}_{$player_key}_{$player_mac}_frame");
			$redis_serv = new Redis();
			$redis_serv->connect('10.171.126.247', 6379);
			$redis_serv->set($redis_key, $frame->fd);
			echo "Connection to server sucessfully\n";
			echo $redis_key . "\n";
		    //查看服务是否运行
			echo "Server is running: " . $redis->ping() . "\n";
   			break;
   		case 'push':
   			break;
   		default:
   			break;
   	}
});

$server->on('close', function ($ser, $fd) {
    echo "client {$fd} closed\n";
});

$server->start();