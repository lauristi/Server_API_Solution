pipeline {
    agent any

    environment {
        LOG_FILE = "pipeline.log"
        GIT_REPO = 'github.com/lauristi/Server_API_Solution.git'
        BRANCH = 'master'
        PROJECT_NAME = 'Server_API'
        SOLUTION_PATH = 'Server_API_Solution'
        PROJECT_PATH_ARCHIVE = 'Server_API/Server_API.csproj'
        PUBLISH_PATH = 'Server_API/bin/Release/net8.0/publish'
        ARTIFACT_PATH = 'Server_API/Artifact'
        DEPLOY_PATH = '/var/www/app/ServerProjects/Server_API'
    }

    stages {
        stage('clean') {
            steps {
                script {
                    sh "rm -rf ${env.SOLUTION_PATH}"
                    sh "rm -rf ${env.PUBLISH_PATH}/*"
                    sh "rm -rf ${env.ARTIFACT_PATH}/*"
                }
            }
        }

        stage('01- Checkout') {
            steps {
                script {
                    withCredentials([string(credentialsId: 'JENKINS_TOKEN', variable: 'GITHUB_TOKEN')]) {
                        sh """
                            git clone https://${GITHUB_TOKEN}@${GIT_REPO}
                            cd ${env.SOLUTION_PATH}
                            git checkout ${BRANCH}
                        """
                    }
                }
            }
        }

        stage('02- Restore Dependencies') {
            steps {
                sh "dotnet restore ${env.PROJECT_NAME}"
            }
        }

        stage('03- Build') {
            steps {
                sh "dotnet build ${env.PROJECT_NAME} --no-restore --configuration Release"
            }
        }

        stage('04- Test') {
            steps {
                sh "dotnet test ${env.PROJECT_NAME} --no-build --verbosity normal"
            }
        }

        stage('05- Publish') {
            steps {
                sh "dotnet publish ${env.PROJECT_PATH_ARCHIVE} -c Release -o ${env.PUBLISH_PATH}"
            }
        }

        stage('06- Package Artifacts') {
            steps {
                sh """
                    mkdir -p ${env.ARTIFACT_PATH}
                    cp -r ${env.PUBLISH_PATH}/* ${env.ARTIFACT_PATH}/
                """
                archiveArtifacts artifacts: "${env.ARTIFACT_PATH}/**", allowEmptyArchive: true
            }
        }

        stage('07- Deploy on server') {
            steps {
                sh """
                    sudo cp -r "${env.ARTIFACT_PATH}"/* "${env.DEPLOY_PATH}/"
                    sudo chown -R www-data:www-data "${env.DEPLOY_PATH}/"
                    sudo systemctl restart kestrel-${env.PROJECT_NAME}.service
                """
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: "${env.LOG_FILE}", allowEmptyArchive: true
            cleanWs()
        }
    }
}
