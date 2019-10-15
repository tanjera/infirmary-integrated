<?php

include '../infirmary-server-access.php';

$conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

if ($conn->connect_error) {
    die();
}

try {
    if ($sql = $conn->prepare(
            "INSERT INTO exceptions" .
            "(timestamp, ii_version, client_os, exception_message, exception_method, exception_stacktrace, exception_hresult, exception_data) " .
            "VALUES" .
            "(?, ?, ?, ?, ?, ?, ?, ?)")) {

        $get_timestamp = $_GET['timestamp'] ?? "";
        $get_ii_version = $_GET['ii_version'] ?? "";
        $get_client_os = $_GET['client_os'] ?? "";
        $get_exception_message = $_GET['exception_message'] ?? "";
        $get_exception_method = $_GET['exception_method'] ?? "";
        $get_exception_stacktrace = $_GET['exception_stacktrace'] ?? "";
        $get_exception_hresult = $_GET['exception_hresult'] ?? "";
        $get_exception_data = $_GET['exception_data'] ?? "";

        $sql->bind_param("ssssssss",
                $get_timestamp,
                $get_ii_version,
                $get_client_os,
                $get_exception_message,
                $get_exception_method,
                $get_exception_stacktrace,
                $get_exception_hresult,
                $get_exception_data);

        $sql->execute();
        $sql->close();
    }

    $conn->close();
} catch (Exception $e) {
    die();
}