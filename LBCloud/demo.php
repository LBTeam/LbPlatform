<?php
$file = "./test.png";
$filesize = filesize($file);
var_dump($filesize);
$content = file_get_contents($file);
var_dump(strlen($content));
$f = fopen($file, 'r+');
$content = fread($f, $filesize);
var_dump(strlen($content));
fclose($f);
exit;


$file = "./57c546fcbfdb0.playprog";
echo strtoupper(md5_file($file));
exit;

$requrest_uri = "http://127.0.0.1/public.bestfu.com/index.php?s=/Home/Weather/getWeatherFromBaidu&province=&city=±±¾©&district=&sid=731rti1m5godcf386815jsasr5";
$result = file_get_contents($requrest_uri);
var_dump($result);