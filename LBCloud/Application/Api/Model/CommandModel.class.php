<?php
/**
 * 命令model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class CommandModel extends Model
{
	/**
	 * 删除命令
	 * @param int	$user_id 用户ID
	 * @param array	$screens 屏幕ID
	 * @param int	$type 命令类型
	 * 					  |0-发布播放方案
	 *					  |1-长连接重新注册
	 * @param int	$status 命令状态
	 * 						|0-已发布（未下发）
	 * 						|1-已下发
	 * @return int
	 */
	public function remove_cmd($user_id, $screens, $type=0, $status=0){
		$map = array();
		$map['user_id'] = $user_id;
		$map['screen_id'] = array("IN", $screens);
		$map['type'] = $type;
		$map['status'] = $status;
		return $this->where($map)->delete();
	}
	
	/**
	 * 发布命令
	 */
	public function release_cmd($cmds){
		return $this->addAll($cmds);
	}
	
	/**
	 * 命令列表
	 */
	public function cmds_list($user_id, $screen_id){
		$map = array();
		$map['user_id'] = $user_id;
		$map['screen_id'] = $screen_id;
		$map['status'] = 0;
		return $this->where($map)->select();
	}
}
