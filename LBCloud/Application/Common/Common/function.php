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
		$code_array = array(
			"0","1","2","3","4","5","6","7","8","9",
			"a", "b", "c", "d", "e", "f", "g",
			"h", "i", "j", "k", "l", "m", "n",
			"o", "p", "q", "r", "s", "t",
			"u", "v", "w", "x", "y", "z",
			"A", "B", "C", "D", "E", "F", "G",
			"H", "I", "J", "K", "L", "M", "N",
			"O", "P", "Q", "R", "S", "T",
			"U", "V", "W", "X", "Y", "Z",
			);
	}
	$code_length = count($code_array) - 1;
	$code = "";
	for($i=0; $i<$length; $i++){
		$code .= $code_array[mt_rand(0, $code_length)];
	}
	return $code;
}