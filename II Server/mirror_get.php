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

    $sql->bind_param("ss",
            filter_input(INPUT_GET, 'accession'),
            filter_input(INPUT_GET, 'accesshash'));
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
