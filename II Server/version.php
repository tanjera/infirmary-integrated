<?php

include '../infirmary-server-access.php';

$conn = new mysqli($dbhost, $dbuser, $dbpass, $dbname);

if ($conn->connect_error) {
    die();
}

try {
    $sql = "SELECT version, upgradeuri_html, upgradeuri_win, upgradehash_win "
            . "FROM versioning ORDER BY accession DESC LIMIT 1";


    $result = $conn->query($sql);

    if ($result->num_rows > 0) {
        while ($row = $result->fetch_assoc()) {
            printf("%s\n%s\n%s\n%s",
                    $row["version"],
                    $row["upgradeuri_html"],
                    $row["upgradeuri_win"],
                    $row["upgradehash_win"]);
        }
    }
} catch (Exception $e) {
    die();
}

$conn->close();
