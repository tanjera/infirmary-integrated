<?php

include '../infirmary-server-access.php';

$conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

if ($conn->connect_error) {
    die();
}

try {
    if ($sql = $conn->prepare(
            "INSERT INTO usage_statistics" .
            "(timestamp, ii_version, env_os, env_lang, client_ip, client_mac, client_user) " .
            "VALUES" .
            "(?, ?, ?, ?, ?, ?, ?)")) {

        $get_timestamp = $_GET['timestamp'] ?? "";
        $get_version = $_GET['ii_version'] ?? "";
        $get_os = $_GET['env_os'] ?? "";
        $get_lang = $_GET['env_lang'] ?? "";
        $get_ip = $_GET['client_ip'] ?? "";
        $get_mac = $_GET['client_mac'] ?? "";
        $get_user = $_GET['client_user'] ?? "";

        $sql->bind_param("sssssss",
                $get_timestamp,
                $get_version,
                $get_os,
                $get_lang,
                $get_ip,
                $get_mac,
                $get_user);

        $sql->execute();
        $sql->close();
    }

    $conn->close();
} catch (Exception $e) {
    die();
}