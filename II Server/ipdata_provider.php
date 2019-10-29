<?php

include '../infirmary-server-access.php';

try {
    $get_ipaddress = $_GET['ip_address'] ?? "";

    if ($get_ipaddress == "") {
        die();
    }

    $req_uri = sprintf("https://api.ipdata.co/%s?api-key=%s",
            $get_ipaddress,
            $ipdata_apikey);

    $req_handle = fopen($req_uri, "r");
    echo stream_get_contents($req_handle);
} catch (Exception $e) {
    die();
}
