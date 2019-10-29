<?php

include '../infirmary-server-access.php';

$conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

if ($conn->connect_error) {
    die();
}

try {
    if ($sql = $conn->prepare(
            "INSERT INTO usage_statistics" .
            "(timestamp, ii_version, env_os, env_lang, client_lang, client_country, client_ip, client_mac, client_user) " .
            "VALUES" .
            "(?, ?, ?, ?, ?, ?, ?, ?, ?)")) {

        $get_timestamp = $_GET['timestamp'] ?? "";
        $get_ii_version = $_GET['ii_version'] ?? "";
        $get_env_os = $_GET['env_os'] ?? "";
        $get_env_lang = $_GET['env_lang'] ?? "";
        $get_client_lang = $_GET['client_lang'] ?? "";
        $get_client_country = $_GET['client_country'] ?? "";
        $get_client_ip = $_GET['client_ip'] ?? "";
        $get_client_mac = $_GET['client_mac'] ?? "";
        $get_client_user = $_GET['client_user'] ?? "";

        $sql->bind_param("sssssssss",
                $get_timestamp,
                $get_ii_version,
                $get_env_os,
                $get_env_lang,
                $get_client_lang,
                $get_client_country,
                $get_client_ip,
                $get_client_mac,
                $get_client_user);

        $sql->execute();
        $sql->close();
    }

    $conn->close();
} catch (Exception $e) {
    die();
}