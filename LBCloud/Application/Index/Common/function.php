<?php
/**
 * 是否登录
 */
function is_login(){
    $user = session('user_auth');
    if (empty($user)) {
        return 0;
    } else {
        return session('user_auth_sign') == data_auth_sign($user) ? $user['uid'] : 0;
    }
}

/**
 * 是否管理员
 */
function is_administrator($uid = null){
    $uid = is_null($uid) ? is_login() : $uid;
    return $uid && (intval($uid) === C('USER_ADMINISTRATOR'));
}

/**
 * 验证码
 */
function check_verify($code, $id = 1){
    $verify = new \Think\Verify();
    return $verify->check($code, $id);
}

/**
 * 数据签名认证
 * @param  array  $data 被认证的数据
 * @return string       签名
 */
function data_auth_sign($data) {
    //数据类型检测
    if(!is_array($data)){
        $data = (array)$data;
    }
    ksort($data); //排序
    $code = http_build_query($data); //url编码并生成query字符串
    $sign = sha1($code); //生成签名
    return $sign;
}

/**
 * 数据替换
 */
function int_to_string(&$data,$map=array('status'=>array(1=>'正常',-1=>'删除',0=>'禁用',2=>'未审核',3=>'草稿'))) {
    if($data === false || $data === null ){
        return $data;
    }
    $data = (array)$data;
    foreach ($data as $key => $row){
        foreach ($map as $col=>$pair){
            if(isset($row[$col]) && isset($pair[$row[$col]])){
                $data[$key][$col.'_text'] = $pair[$row[$col]];
            }
        }
    }
    return $data;
}

/**
 * 检查开始结束时间是否冲突
 */
function check_work_time(){
	$start = strtotime(I("post.start"));
	$end = strtotime(I("post.end"));
	if($start > $end){
		return false;
	}else{
		return true;
	}
}

/**
 * 检查价格时段是否冲突
 */
function check_price_cross(){
	$id = I("post.id", 0);
	$sid = I("post.screen_id");
	$start = I("post.start");
	$end = I("post.end");
	$price_model = D("Price");
	$prices = $price_model->price_by_screen($sid, $id);
	if($prices){
		$cross = true;
		foreach($prices as $val){
			$s1 = strtotime($val['start']);
			$e1 = strtotime($val['end']);
			$s2 = strtotime($start);
			$e2 = strtotime($end);
			$temp = time_is_cross($s1, $e1, $s2, $e2);
			if(!$temp){
				$cross = false;
				break;
			}
		}
		return $cross;
	}else{
		return true;
	}
}

/**
 * 判断俩个时间段是否交叉
 * @param $s1 第一个时间段开始时间戳
 * @param $e1 第一个时间段结束时间戳
 * @param $s2 第二个时间段开始时间戳
 * @param $e2 第二个时间段结束时间戳
 * @return boolen  false-有交叉,true-无交叉
 */
function time_is_cross($s1, $e1, $s2, $e2)
{
	$t1 = $s2 - $s1;
	if ($t1 > 0){
		$t2 = $s2 - $e1;
		if ($t2 >= 0){
			return true;
		}else{
			return false;
		}
	}else{
		$t2 = $e2 - $s1;
		if ($t2 > 0){
			return false;
		}else{
			return true;
		}
	}
}

/**
 * 检查注册码是否存在
 */
function check_regcode(){
	$code = I("post.reg_code", "");
	if($code){
		$user_model = D("User");
		$map = array();
		$map['reg_code'] = $code;
		$map['uid'] = array("NEQ", ADMIN_UID);
		$count = $user_model->where($map)->count();
		if($count == 0){
			return true;
		}else{
			return false;
		}
	}else{
		return false;
	}
}

/**
 * 检查代理商注册码
 */
function check_agent_code(){
	$code = I("post.reg_code");
	$user_model = D("User");
	$map = array("reg_code" => $code);
	$count = $user_model->where($map)->count();
	if($count > 0){
		return true;
	}else{
		return false;
	}
}

/**
 * 检查手机短信码
 */
function check_smscode(){
	$phone = I("post.phone", "");
	$code = I("post.code", "");
	if($phone && $code){
		$smscode = session("{$phone}_code");
		session("{$phone}_code", null);
		return $smscode && $smscode == $code;
	}else{
		return false;
	}
}

/**
 * 检查手机号是否存在
 */
function check_phone(){
	$phone = I("post.phone", "");
	if($phone){
		$user_model = D("User");
		$map = array("phone" => $phone);
		$count = $user_model->where($map)->count();
		if($count > 0){
			return true;
		}else{
			return false;
		}
	}else{
		return false;
	}
}

/**
 * 检查邮箱是否存在
 */
function check_email(){
	$email = I("post.email", "");
	if($email){
		$user_model = D("User");
		$map = array("email" => $email);
		$count = $user_model->where($map)->count();
		if($count > 0){
			return true;
		}else{
			return false;
		}
	}else{
		return false;
	}
}

/**
 * 生成数据防篡改签名
 * @return string
 */
function create_data_sign($data, $key=''){
	if(empty($key)){
        $key = C("DATA_SIGN_KEY");
    }
	$data = http_build_query($data);
	$sign = sha1(md5("{$data}{$key}"));
	return $sign;
}

/**
 * 生成参数请求字符串
 */
function http_param_query($data=array()){
	if($data){
		$array = array();
		foreach($data as $key => $val){
			$array[] = "{$key}={$val}";
		}
		$string = implode("&", $array);
		return $string;
	}
	return "";
}
