<?php

include '../infirmary-server-access.php';

$conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

if ($conn->connect_error) {
    die();
}

try {
    $sql = "SELECT version, upgradeuri_html "
            . "FROM versioning ORDER BY accession DESC LIMIT 1";


    $result = $conn->query($sql);

    if ($result->num_rows > 0) {
        while ($row = $result->fetch_assoc()) {
            printf("%s\n%s",
                    $row["version"],
                    $row["upgradeuri_html"]);
        }
    }
} catch (Exception $e) {
    die();
}

$conn->close();
