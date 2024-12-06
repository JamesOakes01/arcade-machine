import os
import git
from git import Repo
import csv





#check for new games that haven't been pulled yet
def check_for_new(repo_list, repo_dir):
    with open(repo_list, newline='') as csvfile:
        reader = csv.reader(csvfile)
        for row in reader:
            name = row[0].split('/')[-1]
            print(name)
            if not os.path.isdir(repo_dir + '/' + name):
                Repo.clone_from(''.join(row) + '.git', repo_dir + "/" + name)


def update_repo(repo_path):
    repo = git.Repo(repo_path)
    o = repo.remotes.origin
    o.pull()


def run():
    REPOLIST = "GameRepos.csv"
    REPODIR = "GameBuilds"
    LAUNCHERPATH = "Build\Arcade-Machine.exe"
    
    check_for_new(REPOLIST, REPODIR)
    
    
    dirs = [os.path.join(REPODIR, o) for o in os.listdir(REPODIR) if os.path.isdir(os.path.join(REPODIR,o))]
    
    for entry in dirs:
        update_repo(entry)
        
    os.startfile(LAUNCHERPATH)
