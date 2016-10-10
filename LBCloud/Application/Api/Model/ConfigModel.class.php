<?php
/**
 * 系统配置model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class ConfigModel extends Model
{
	public function ws_config(){
		$ws = array();
		$cfgs = $this->getField("key,value");
		$ws['ip'] = $cfgs['WEB_SOCKET_IP'] ? $cfgs['WEB_SOCKET_IP'] : C("websocket_ip");
		$ws['port'] = $cfgs['WEB_SOCKET_PORT'] ? $cfgs['WEB_SOCKET_PORT'] : C("websocket_port");
		return $ws;
	}
}
