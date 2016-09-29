<?php
/**
 * 产生随机字符串
 * @param $length 字符串长度
 * @param $num 是否纯数字
 * @return string
 */
function random_string($length, $num=false){
	if($num){
		$code_array = array("0","1","2","3","4","5","6","7","8","9");
	}else{
		//"0","1","l","i","o","L","I","O",
		$code_array = array(
			"2", "3", "4", "5", "6", "7", "8", "9",
			"a", "b", "c", "d", "e", "f", "g", "h",
			"j", "k", "m", "n", "p", "q", "r", "s",
			"t", "u", "v", "w", "x", "y", "z",
			"A", "B", "C", "D", "E", "F", "G", "H",
			"J", "K", "M", "N", "P", "Q", "R", "S",
			"T", "U", "V", "W", "X", "Y", "Z"
		);
	}
	$code_length = count($code_array) - 1;
	$code = "";
	for($i=0; $i<$length; $i++){
		$code .= $code_array[mt_rand(0, $code_length)];
	}
	return $code;
}

/**
 * 获取oss存储对象名称
 * @param $subfix 后缀名
 * @return string
 */
function oss_object($subfix){
	$object = date('Ymd') . "/" . uniqid() . (!empty($subfix) ? ".$subfix": '');
	return $object;
}

//===================协议加密解密start========================
$AES_Key = "NeBnjf3TqsDERvafpO4xyNAk2ZPjVRs=";
$AES_IV = "AHmAq1tzTtKlOJE03IQ8t82SXxFloPq=";
define ( "AES_Key", $AES_Key );
define ( "AES_IV", $AES_IV );
function addpadding($string, $blocksize = 32) {
    $len = strlen ( $string );
    $pad = $blocksize - ($len % $blocksize);
    $string .= str_repeat ( chr ( $pad ), $pad );
    return $string;
}
function strippadding($string) {
    $slast = ord ( substr ( $string, - 1 ) );
    $slastc = chr ( $slast );
    $pcheck = substr ( $string, - $slast );
    if (preg_match ( "/$slastc{" . $slast . "}/", $string )) {
        $string = substr ( $string, 0, strlen ( $string ) - $slast );
        return $string;
    } else {
        return false;
    }
}
function encrypt($string = "") {
    $key = utf8_decode ( AES_Key );
    $iv = utf8_decode ( AES_IV );

    return base64_encode ( mcrypt_encrypt ( MCRYPT_RIJNDAEL_256, $key, addpadding ( $string ), MCRYPT_MODE_CBC, $iv ) );
}
function decrypt($string = "") {
    $key = utf8_decode ( AES_Key );
    $iv = utf8_decode ( AES_IV );
    $string = base64_decode ( $string );
    return strippadding ( mcrypt_decrypt ( MCRYPT_RIJNDAEL_256, $key, $string, MCRYPT_MODE_CBC, $iv ) );
}
//===================协议加密解密end========================

/**
 * 密码加密方法
 * @param string $pw 要加密的字符串
 * @param string $key 加密密钥
 * @return string
 */
function sp_password($pw, $key = ''){
    if(empty($key)){
        $key = C("pwd_auth_key");
    }
	$result = "###" . md5(md5("{$key}{$pw}"));
	return $result;
}

/**
 * 密码比较方法,所有涉及密码比较的地方都用这个方法
 * @param string $password 要比较的密码
 * @param string $password_in_db 数据库保存的已经加密过的密码
 * @return boolean 密码相同，返回true
 */
function sp_compare_password($password, $password_in_db){
	return sp_password($password) == $password_in_db;
}

/**
 * 生成用户token
 * @return array
 */
function create_access_token($username){
	$random = uniqid();
	$time = time()+7200;
	$token = sha1("{$username}_{$random}_{$time}");
	return array('token'=>$token, 'expire'=>$time);
}