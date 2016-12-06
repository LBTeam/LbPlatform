<?php
/**
 * websocket
 */
error_reporting(0);

$server = new swoole_websocket_server("0.0.0.0", 9501);

$server->on('open', function (swoole_websocket_server $server, $request) {
    echo "server: handshake success with fd{$request->fd}\n";
});

$server->on('message', function (swoole_websocket_server $server, $frame) {
    //echo "receive from {$frame->fd}:{$frame->data},opcode:{$frame->opcode},fin:{$frame->finish}\n";
    /*$server->push($frame->fd, "this is server");*/
   	$data = json_decode($frame->data, true);
	$player_id = $data['Id'];
	$player_key = $data['Key'];
	$player_mac = strtoupper(str_replace(':', '-', $data['Mac']));
	$redis_key = md5("{$player_id}_{$player_key}_{$player_mac}_fd");
	$redis_serv = new Redis();
	$redis_serv->connect('10.171.126.247', 6379);
   	switch($data['Act']){
   		case 'bind':
			$res = $redis_serv->set($redis_key, $frame->fd);
			$response = array();
			if($res){
				$fd_key = md5("player_fd_".$frame->fd);
				$redis_serv->set($fd_key, "{$player_id}_{$player_key}_{$player_mac}");
				$response = array("err_code"=>"000000","msg"=>"ok");
			}else{
				$response = array("err_code"=>"100001","msg"=>"failure");
			}
			$server->push($frame->fd, json_encode($response));
   			break;
   		case 'notice':
			//紧急通知
			$client_fd = $redis_serv->get($redis_key);
			if($client_fd){
				$client_resp = array("Act"=>1,"Msg"=>$data['Content']);
				//通知播放端
				$client_res = $server->push($client_fd, json_encode($client_resp));
				if($client_res){
					//$response = array("err_code"=>"000000","msg"=>"ok");
					//通知web端
					$server->push($frame->fd, true);
				}else{
					//$response = array("err_code"=>"100001","msg"=>"failure");
					//通知web端
					$server->push($frame->fd, false);
				}
			}else{
				//$response = array("err_code"=>"100001","msg"=>"failure");
				//通知web端
				$server->push($frame->fd, false);
			}
   			break;
		case 'shutdown':
			//紧急关闭
			$client_fd = $redis_serv->get($redis_key);
			if($client_fd){
				$client_resp = array("Act"=>2,"Msg"=>"");
				//通知播放端
				$client_res = $server->push($client_fd, json_encode($client_resp));
				if($client_res){
					$fd_key = md5("player_fd_{$client_fd}");
					$redis_serv->del($redis_key);
					$redis_serv->del($fd_key);
					//$response = array("err_code"=>"000000","msg"=>"ok");
					//通知web端
					$server->push($frame->fd, true);
				}else{
					//$response = array("err_code"=>"100001","msg"=>"failure");
					//通知web端
					$server->push($frame->fd, false);
				}
			}else{
				//$response = array("err_code"=>"100001","msg"=>"failure");
				//通知web端
				$server->push($frame->fd, false);
			}
			break;
   		default:
   			break;
   	}
});

$server->on('close', function ($ser, $fd) {
    //echo "client {$fd} closed\n";
	$fd_key = md5("player_fd_{$fd}");
	$redis_serv = new Redis();
	$redis_serv->connect('10.171.126.247', 6379);
	$player = $redis_serv->get($fd_key);
	$redis_key = md5("{$player}_fd");
	$redis_serv->del($fd_key);
	$redis_serv->del($redis_key);
	unset($redis_serv);
});

$server->start();