<?php

include '../infirmary-server-access.php';

$conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

if ($conn->connect_error) {
    die();
}

try {
    $sql = $conn->prepare(
            "SELECT key_edit FROM mirrors WHERE accession = ?");

    $sql->bind_param("s", filter_input(INPUT_GET, 'accession'));
    $sql->bind_result($existing);
    $sql->execute();


    $row_exists = false;
    if ($sql->fetch()) {            // An entry exists for this accession
        if ($existing != filter_input(INPUT_GET, 'key_edit')) {
            $conn->close();
            die();
        } else if ($existing == filter_input(INPUT_GET, 'key_edit')) {
            if ($sql = $conn->prepare(
                    "UPDATE mirrors SET key_access = ?, key_edit = ?, " .
                    "patient = ?, updated = ?, client_ip = ?, client_user = ? " .
                    "WHERE accession = ?")) {

                $sql->bind_param("sssssss",
                        filter_input(INPUT_GET, 'key_access'),
                        filter_input(INPUT_GET, 'key_edit'),
                        filter_input(INPUT_GET, 'patient'),
                        filter_input(INPUT_GET, 'updated'),
                        filter_input(INPUT_GET, 'client_ip'),
                        filter_input(INPUT_GET, 'client_user'),
                        filter_input(INPUT_GET, 'accession'));

                $sql->execute();
                $sql->close();
            }

            $conn->close();
        }
    } else if (!$sql->fetch()) {    // No entry exists for this accession
        if ($sql = $conn->prepare(
                "INSERT INTO mirrors " .
                "(accession, key_access, key_edit, patient, updated, client_ip, client_user) " .
                "VALUES " .
                "(?, ?, ?, ?, ?, ?, ?)")) {

            $sql->bind_param("sssssss",
                    filter_input(INPUT_GET, 'accession'),
                    filter_input(INPUT_GET, 'key_access'),
                    filter_input(INPUT_GET, 'key_edit'),
                    filter_input(INPUT_GET, 'patient'),
                    filter_input(INPUT_GET, 'updated'),
                    filter_input(INPUT_GET, 'client_ip'),
                    filter_input(INPUT_GET, 'client_user'));

            $sql->execute();
            $sql->close();
        }

        $conn->close();
    }
} catch (Exception $e) {
    die();
}