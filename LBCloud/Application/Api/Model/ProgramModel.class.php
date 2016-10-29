<?php
/**
 * 播放方案model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;
use Api\Service\AliyunOSS;

class ProgramModel extends Model
{
	/**
	 * 用户播放方案
	 * @param $user_id 用户ID
	 * @return array
	 */
	public function all_program($user_id){
		$map = array();
		$map['user_id'] = $user_id;
		$map['status'] = 1;
		return $this->where($map)->select();
	}
	
	
	/**
	 * 检查播放方案是否存在
	 * @param $name 播放方案名称
	 * @param $md5 播放方案md5
	 * @param $user_id 用户ID
	 * @return int|boolen
	 */
	public function program_exists($name, $md5, $user_id){
		$map = array();
		$map['user_id'] = $user_id;
		$map['name'] = mysql_escape_string($name);
		$map['md5'] = $md5;
		$program_id = $this->where($map)->getField('id');
		if($program_id){
			return $program_id;
		}else{
			return false;
		}
	}
	
	/**
	 * 播放方案详情
	 * @param $program_id 播放方案ID
	 * @param $user_id 用户ID
	 * @return array
	 */
	public function program_detail($program_id, $user_id=false){
		$map = array();
		$map['id'] = $program_id;
		if($user_id){
			$map['user_id'] = $user_id;
		}
		return $this->where($map)->find();
	}
	
	/**
	 * 获取播放方案详情
	 * @param $name 播放方案名称
	 * @param $md5 播放方案md5
	 * @param $user_id 用户ID
	 * @return array
	 */
	public function program_by_name_md5($name, $md5, $user_id){
		$map = array();
		$map['user_id'] = $user_id;
		$map['name'] = mysql_escape_string($name);
		$map['md5'] = $md5;
		return $this->where($map)->find();
	}
	
	/**
	 * 检查播放方案是否可以发布
	 * @param $program_id 播放方案ID
	 * @param $user_id 用户ID
	 * @return boolen
	 */
	public function program_can_release($program_id, $user_id){
		if($program_id){
			$program = $this->program_detail($program_id, $user_id);
			if($program){
				$AliyunOSS = new AliyunOSS();
				$program_bucket = C("oss_program_bucket");
				$pro_exists = $AliyunOSS->object_exists($program['object'], $program_bucket);
				if($pro_exists){
					$medias = json_decode($program['info'], true);
					$media_model = D("Media");
					$media_bucket = C("oss_media_bucket");
					$is_release = true;
					foreach($medias as $val){
						$media = $media_model->media_by_name_md5($val['MediaName'], $val['MediaMD5'], $user_id);
						if($media){
							$obj_exists = $AliyunOSS->object_exists($media['object'], $media_bucket);
							if(!$obj_exists){
								$is_release = false;
								break;
							}
						}else{
							$is_release = false;
							break;
						}
					}
					if($is_release){
						return true;
					}else{
						return false;
					}
				}else{
					return false;
				}
			}else{
				return false;
			}
		}else{
			return false;
		}
	}
}
