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

try {
    // Check if:
    // A row exists with the accession key
    // And if so, get the administrator password for editing the row
    $sql = $conn->prepare(
            "SELECT key_edit FROM mirrors WHERE accession = ?");

    $sql->bind_param("s",
            $get_accession);
    $sql->bind_result($existing);
    $sql->execute();

    // Does the row exist?
    $row_exists = $sql->fetch() ? true : false;

    if ($row_exists) {            // An entry exists for this accession

        if ($existing != filter_input(INPUT_GET, 'key_edit')) {
            $conn->close();
            printf("BAD_PASSWORD\n");

            die();

        } else if ($existing == filter_input(INPUT_GET, 'key_edit')) {

            $conn->refresh(MYSQLI_REFRESH_STATUS);
            $sql->reset();

            if ($sql = $conn->prepare(
                    "UPDATE mirrors SET key_access = ?, " .
                    "patient = ?, updated = ? " .
                    "WHERE accession = ? AND key_edit = ?")) {

                $sql->bind_param("sssss",
                        $get_key_access,
                        $get_patient,
                        $get_updated,
                        $get_accession,
                        $get_key_edit);

                $sql->execute();
                $sql->close();

                printf("ENTRY_UPDATED\n");
            }

            $conn->close();
        }
    } else if (!$row_exists) {    // No entry exists for this accession

        $conn->refresh(MYSQLI_REFRESH_STATUS);
        $sql->reset();

        if ($sql = $conn->prepare(
                "INSERT INTO mirrors " .
                "(accession, key_access, key_edit, patient, updated) " .
                "VALUES " .
                "(?, ?, ?, ?, ?)")) {

            $sql->bind_param("sssss",
                    $get_accession,
                    $get_key_access,
                    $get_key_edit,
                    $get_patient,
                    $get_updated);

            $sql->execute();
            $sql->close();

            printf("ENTRY_ADDED\n");
        }

        $conn->close();
    }
} catch (Exception $e) {
    printf("EXCEPTION\n");
    die();
}
