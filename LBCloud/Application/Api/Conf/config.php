<?php
return array(
	//'配置项'=>'配置值'
	'aliyun_oss_id'			=>	'f1mcwCSSqB9tIY57',
	'aliyun_oss_secret'		=>	'7aSeEDlyyV5pIddw49FhLXFMVvG9UK',
	'aliyun_oss_endpoint'	=>	'http://oss-cn-shenzhen.aliyuncs.com',
	'aliyun_oss_bucket'		=>	'lb-player-test',
	'oss_media_bucket'		=>	'lb-player-media',
	'oss_program_bucket'	=>	'lb-player-program',
	
	'oss_part_size'			=>	10485760,	//oss上传分片大小
	'oss_100K_size'			=>	102400,		//100K大小
	'oss_200K_size'			=>	204800,		//100K大小
	'oss_1M_part_size'		=>	1048576,	//1M大小
	'oss_2M_part_size'		=>	2097152,	//2M大小
	'oss_5M_part_size'		=>	5242880,	//5M大小
	'oss_10M_part_size'		=>	10485760,	//10M大小
	'oss_50M_part_size'		=>	52428800,	//50M大小
	
	'player_program_subfix'	=>	'playprog',	//播放方案文件后缀名
	
	//未登录可访问模块
	'not_logged_allow' => array(
		'login',
	),
);