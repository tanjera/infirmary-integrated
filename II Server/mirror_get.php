<?php

include '../infirmary-server-access.php';

$conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

if ($conn->connect_error) {
    die();
}

try {
    $sql = $conn->prepare(
            "SELECT updated, patient FROM mirrors "
            . "WHERE accession = ? AND key_access = ?");

    $get_accession = $_GET['accession'] ?? "";
    $get_accesshash = $_GET['accesshash'] ?? "";

    $sql->bind_param("ss",
            $get_accession,
            $get_accesshash);

    $sql->bind_result($updated, $patient);
    $sql->execute();

    while ($sql->fetch()) {
        printf("%s\n%s\n", $updated, $patient);
    }

    $sql->close();
    $conn->close();
} catch (Exception $e) {
    die();
}
