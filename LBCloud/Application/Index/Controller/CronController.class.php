<?php
/**
 * 定时清理控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;
use Think\Controller;
use Api\Service\AliyunOSS;

class CronController extends Controller
{
	/**
	 * 大文件媒体清理
	 * curl http://lbcloud.ddt123.cn/index.php/Index/Cron/big_file_clean
	 */
	public function big_file_clean(){
		set_time_limit(0);	//执行时间
		ini_set('memory_limit', '256M'); //设置内存
		
		$cfg_model = D("Config");
		$media_model = D("Media");
		$cfgs = $cfg_model->configs();
		$clean_size = $cfgs['CLEAR_SIZE'] * 1024 * 1024;
		$clean_time = strtotime("-{$cfgs['CLEAR_DAY']} day");
		$media_list = $media_model->clean_list($clean_size, $clean_time);
		if($media_list){
			$media_ids = array();
			foreach($media_list as $k => $v){
				$media_ids[] = $k;
			}
			$map = array(
				"id"=>array("IN", $media_ids)
			);
			$AliyunOSS = new AliyunOSS();
			$media_bucket = C("oss_media_bucket");
			$model = new \Think\Model();
			$model->startTrans();
			$delete_obj = $AliyunOSS->delete_objects($media_list, $media_bucket);
			$delete_ids = $media_model->where($map)->delete();
			if($delete_obj && $delete_ids){
				$model->commit();
			}else{
				$model->rollback();
			}
			unset($AliyunOSS);
			unset($model);
		}
		unset($cfg_model);
		unset($media_model);
	}
	
	/**
	 * 监控图片清理
	 * curl http://lbcloud.ddt123.cn/index.php/Index/Cron/monitor_pic_clean
	 */
	public function monitor_pic_clean(){
		//$t1 = microtime(true);
		
		set_time_limit(0);	//执行时间
		ini_set('memory_limit', '256M'); //设置内存
		
		$objs = array();
		$cfg_model = D("Config");
		$cfgs = $cfg_model->configs();
		//$cfgs['MONITOR_DAY'] = 1;
		$clean_time = strtotime("-{$cfgs['MONITOR_DAY']} day");
		$picture_bucket = C("oss_picture_bucket");
		$AliyunOSS = new AliyunOSS();
		$object_count = 1000;
		
		//获取删除监控图片方法1
		$object_marker = false;
		$object_lists = array();
		while(true){
			$object_list = $AliyunOSS->object_list($picture_bucket, '', $object_count, $object_marker);
			if($object_list['prefixs']){
				foreach($object_list['prefixs'] as $object){
					$object_lists[] = $object['prefix'];
				}
				if(count($object_list['prefixs']) == $object_count){
					$temp_marker = end($object_list['prefixs']);
					$object_marker = $temp_marker['prefix'];
				}else{
					break;
				}
			}else{
				break;
			}
		}
		$obj_lists = array();
		if($object_lists){
			foreach($object_lists as $object){
				$obj_marker = false;
				while(true){
					$obj_list = $AliyunOSS->object_list($picture_bucket, $object, $object_count, $obj_marker);
					if($obj_list['prefixs']){
						foreach($obj_list['prefixs'] as $obj){
							$temp = explode('/', $obj['prefix']);
							$temp_time = strtotime($temp[1]);
							if($temp_time < $clean_time){
								$obj_lists[] = $obj['prefix'];
							}
						}
						$end = end($obj_list['prefixs']);
						$end_time = strtotime($end[1]);
						if($end_time < $clean_time){
							if(count($obj_list['prefixs']) == $object_count){
								$temp_marker = end($obj_list['prefixs']);
								$obj_marker = $temp_marker['prefix'];
							}else{
								break;
							}
						}else{
							break;
						}
					}else{
						break;
					}
				}
			}
		}
		if($obj_lists){
			foreach($obj_lists as $obj){
				$o_marker = false;
				while(true){
					$o_list = $AliyunOSS->object_list($picture_bucket, $obj, $object_count, $o_marker);
					if($o_list['objects']){
						foreach($o_list['objects'] as $o){
							$objs[] = $o['key'];
						}
						if(count($o_list['objects']) == $object_count){
							$temp_marker = end($o_list['objects']);
							$o_marker = $temp_marker['key'];
						}else{
							break;
						}
					}else{
						break;
					}
				}
			}
		}
		
		/*
		//获取删除监控图片方法2
		$object_marker = false;
		while(true){
			$object_list = $AliyunOSS->object_list($picture_bucket, '', $object_count, $object_marker);
			if($object_list['prefixs']){
				foreach($object_list['prefixs'] as $object){
					$obj_marker = false;
					while(true){
						$obj_list = $AliyunOSS->object_list($picture_bucket, $object['prefix'], $object_count, $obj_marker);
						if($obj_list['prefixs']){
							foreach($obj_list['prefixs'] as $obj){
								$o_marker = false;
								$temp = explode('/', $obj['prefix']);
								$temp_time = strtotime($temp[1]);
								if($temp_time < $clean_time){
									//需要清理的文件夹
									while(true){
										$o_list = $AliyunOSS->object_list($picture_bucket, $obj['prefix'], $object_count, $o_marker);
										if($o_list['objects']){
											foreach($o_list['objects'] as $o){
												$objs[] = $o['key'];
											}
											if(count($o_list['objects']) == $object_count){
												$temp_marker = end($o_list['objects']);
												$o_marker = $temp_marker['key'];
											}else{
												break;
											}
										}else{
											break;
										}
									}
								}else{
									break;
								}
							}
							$end = end($obj_list['prefixs']);
							$end_time = strtotime($end[1]);
							if($end_time < $clean_time){
								if(count($obj_list['prefixs']) == $object_count){
									$temp_marker = end($obj_list['prefixs']);
									$obj_marker = $temp_marker['prefix'];
								}else{
									break;
								}
							}else{
								break;
							}
						}else{
							break;
						}
					}
				}
				if(count($object_list['prefixs']) == $object_count){
					$temp_marker = end($object_list['prefixs']);
					$object_marker = $temp_marker['prefix'];
				}else{
					break;
				}
			}else{
				break;
			}
		}
		*/
		
		//$t2 = microtime(true);
		//dump('耗时'.round($t2-$t1,3).'秒');
		
		if($objs){
			$AliyunOSS->delete_objects($objs, $picture_bucket);
		}
		unset($cfg_model);
		unset($AliyunOSS);
	}
}
