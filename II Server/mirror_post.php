<?php

include '../infirmary-server-access.php';

$conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

if ($conn->connect_error) {
    die();
}

$get_accession = $_GET['accession'] ?? "";
$get_key_access = $_GET['key_access'] ?? "";
$get_key_edit = $_GET['key_edit'] ?? "";
$get_patient = $_GET['patient'] ?? "";
$get_updated = $_GET['updated'] ?? "";
$get_client_ip = $_GET['client_ip'] ?? "";
$get_client_user = $_GET['client_user'] ?? "";

try {
    $sql = $conn->prepare(
            "SELECT key_edit FROM mirrors WHERE accession = ?");

    $sql->bind_param("s",
            $get_accession);
    $sql->bind_result($existing);
    $sql->execute();

    $row_exists = false;
    if ($sql->fetch()) {            // An entry exists for this accession
        if ($existing != filter_input(INPUT_GET, 'key_edit')) {
            $conn->close();
            die();
        } else if ($existing == filter_input(INPUT_GET, 'key_edit')) {

            $conn->refresh();
            $sql->reset();

            if ($sql = $conn->prepare(
                    "UPDATE mirrors SET key_access = ?, key_edit = ?, " .
                    "patient = ?, updated = ?, client_ip = ?, client_user = ? " .
                    "WHERE accession = ?")) {

                $sql->bind_param("sssssss",
                        $get_key_access,
                        $get_key_edit,
                        $get_patient,
                        $get_updated,
                        $get_client_ip,
                        $get_client_user,
                        $get_accession);

                $sql->execute();
                $sql->close();
            }

            $conn->close();
        }
    } else if (!$sql->fetch()) {    // No entry exists for this accession
        $conn->refresh();
        $sql->reset();

        if ($sql = $conn->prepare(
                "INSERT INTO mirrors " .
                "(accession, key_access, key_edit, patient, updated, client_ip, client_user) " .
                "VALUES " .
                "(?, ?, ?, ?, ?, ?, ?)")) {

            $sql->bind_param("sssssss",
                    $get_accession,
                    $get_key_access,
                    $get_key_edit,
                    $get_patient,
                    $get_updated,
                    $get_client_ip,
                    $get_client_user);

            $sql->execute();
            $sql->close();
        }

        $conn->close();
    }
} catch (Exception $e) {
    die();
}