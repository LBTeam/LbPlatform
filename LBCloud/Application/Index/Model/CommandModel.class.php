<?php
/**
 * 命令model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class CommandModel extends Model
{
	/**
	 * 删除命令
	 * @param array	$screen_id 屏幕ID
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
	public function rm_cmd_by_sid($screen_id, $type=0, $status=0){
		$map = array();
		$map['screen_id'] = $screen_id;
		$map['type'] = $type;
		$map['status'] = $status;
		return $this->where($map)->delete();
	}
}
