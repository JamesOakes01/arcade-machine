import os
import git
from git import Repo
import gamesUpdater

def update_from_git(repo_path, branch="main"):
    """Updates the code from a Git repository."""

    try:
        repo = Repo(repo_path)
        origin = repo.remotes.origin
        origin.fetch()

        if repo.active_branch.name != branch:
            print(f"Switching to branch {branch}")
            repo.git.checkout(branch, force = True)

        print("Pulling changes from remote...")
        origin.pull()

        print("Update successful.")
        return True

    except git.exc.GitCommandError as e:
        print(f"Error updating from Git: {e}")
        return False

repo_path = os.path.dirname(os.path.abspath(__file__))
update_from_git(repo_path)
gamesUpdater.run()