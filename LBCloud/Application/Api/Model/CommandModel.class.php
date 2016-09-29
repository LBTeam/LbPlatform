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
	 * @param array	$screens 屏幕ID
	 * @param int	$type 命令类型
	 * 					  |0-发布播放方案
	 * 					  |1-锁定屏幕参数更新
	 * 					  |2-心跳周期更新
	 * 					  |3-监控数据上传参数更新
	 * 					  |4-软件定时开关时间更新
	 * 					  |5-屏幕参数更新
	 * 					  |6-屏幕工作时间更新
	 * 					  |7-播放方案紧急插播
	 * 					  |8-离线策略
	 * 					  |9-终端长连接重连
	 * @param int	$status 命令状态
	 * 						|0-已发布（未下发）
	 * 						|1-已下发
	 * @return int
	 */
	public function remove_cmd($screens, $type=0, $status=0){
		$map = array();
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
	public function cmds_list($screen_id){
		$map = array();
		$map['screen_id'] = $screen_id;
		$map['status'] = 0;
		return $this->where($map)->select();
	}
	
	/**
	 * 更改命令为已下发
	 */
	public function cmd_issued($cmd_ids){
		if($cmd_ids){
			if(!is_array($cmd_ids)){
				$cmd_ids = explode(',', $cmd_ids);
			}
			$map = array();
			$map['id'] = array('IN', $cmd_ids);
			return $this->where($map)->setField("status", 1);
		}
		return true;
	}
}
